using SmidtFramework.LoggingSystem;
using System;
using System.Globalization;
using System.IO;
using System.Xml;
using UnityEngine;
using VisualizingTheDatabike.DataModel;

namespace VisualizingTheDatabike.Services
{
	/// <summary>
	/// Service for reading a timeline from a .gpx file. Now with less spaghetti. Uses an internal state machine to step through the gpx file.
	/// </summary>
	public class XMLTimelineReader : ITimelineReader
	{
		#region types
		private enum State
		{
			start,
			metadata,
			meta_extensions,
			post_meta,
			trk,
			trkpt,
			trkpt_extensions
		}

		private struct Trackpoint
		{
			public float? lat;
			public float? lon;
			public float? ele;
			public long? time;
			public float? absrotx;
			public float? absroty;
			public float? absrotz;
			public float? addrotx;
			public float? addroty;
			public float? addrotz;
			public float? wheelrpm;
			public float? steeringrot;
			public float? pedalrot;
			public int? pedalrotdir;
			public int? gearfront;
			public int? gearrear;
			public float? brakeright;
			public float? brakeleft;
			public float? suspfront;
			public float? susprear;
			public float? seatpos;

			public bool IsComplete()
			{
				return (lat ?? lon ?? ele ?? time ?? addrotx ?? addroty ?? addrotz ?? wheelrpm ?? steeringrot ?? pedalrot ?? pedalrotdir ?? gearfront ?? gearrear ?? brakeright ?? brakeleft ?? suspfront ?? susprear ?? seatpos) != null;
			}

			public void Clear()
			{
				lat = null;
				lon = null;
				ele = null;
				time = null;
				absrotx = null;
				absroty = null;
				absrotz = null;
				addrotx = null;
				addroty = null;
				addrotz = null;
				wheelrpm = null;
				steeringrot = null;
				pedalrot = null;
				pedalrotdir = null;
				gearfront = null;
				gearrear = null;
				brakeright = null;
				brakeleft = null;
				suspfront = null;
				susprear = null;
				seatpos = null;
			}

			public override string ToString()
			{
				return "Timestamp: lat " + lat + "; lon " + lon + "; ele " + ele + "; time " + time + "; absrotx " + absrotx + "; absroty " + absroty + "; absrotz " + absrotz + "; addrotx " + addrotx + "; addroty " + addroty + "; addrotz " + addrotz + "; wheelrpm " + wheelrpm + "; steeringrot " + steeringrot + "; pedalrot " + pedalrot + "; pedalrotdir " + pedalrotdir + "; gearfront " + gearfront + "; gearrear " + gearrear + "; brakeright " + brakeright + "; brakeleft " + brakeleft + "; suspfront " + suspfront + "; susprear " + susprear + "; seatpos " + seatpos;
			}
		}
		#endregion

		#region fields
		//If additive rotation differs more than this (in all axis combined) from the read absolute rotation an error is logged
		private const float rotationErrorLimit = 10f;
		private bool DEBUG;
		private bool EXTENDED;

		private RawTimeline timeline; //readout buffer
		private UnitConverter converter;
		private BikeDefinition bikeDefinition;

		private State currentState;
		private Trackpoint currentTrkpt = new Trackpoint();
		private Vector3 previousRotation;
		private float previousPedalRotation;
		private bool isEndReached;
		#endregion

		#region public_functions
		/// <summary>
		/// Reads and parses the .gpx file from filepath and returns the result as a Timeline. Throws an InvalidDataException on parsing error.
		/// </summary>
		/// <param name="filepath">Path to the .gpx file.</param>
		/// <returns>A Timeline containing all of the raw data.</returns>
		public RawTimeline ReadTimelineFromFile(string filepath, out BikeDefinition bikeDef)
		{
			DEBUG = GlobalValues.DebugTimelineReader;
			EXTENDED = GlobalValues.DebugTimelineReaderExtended;

			//reset internal state
			timeline = new RawTimeline();
			converter = new UnitConverter();
			currentState = State.start;
			previousRotation = new Vector3(0, 180, 0);
			previousPedalRotation = 0f;
			isEndReached = false;

			using (XmlReader reader = XmlReader.Create(File.OpenRead(filepath)))
			{
				try
				{
					while (!isEndReached && reader.Read())
					{
						string name = reader.Name.ToLowerInvariant();

						if (reader.NodeType == XmlNodeType.Element)
						{
							switch (currentState)
							{
								case State.start:
									ProcessOpeningStart(name);
									break;
								case State.metadata:
									ProcessOpeningMetadata(name, reader);
									break;
								case State.meta_extensions:
									ProcessOpeningMetadataExtensions(name, reader);
									break;
								case State.post_meta:
									ProcessOpeningPostMeta(name);
									break;
								case State.trk:
									ProcessOpeningTrk(name, reader);
									break;
								case State.trkpt:
									ProcessOpeningTrkpt(name, reader);
									break;
								case State.trkpt_extensions:
									ProcessOpeningTrkptExtensions(name, reader);
									break;
							}
						}
						else if (reader.NodeType == XmlNodeType.EndElement)
						{
							switch (currentState)
							{
								case State.start:
									LSLogger.LogError("TimelineReader Start state encountered clsoing tag '" + name + "'.");
									throw new InvalidDataException("Invalid timeline format.");

								case State.metadata:
									LSLogger.LogError("TimelineReader Metadata state encountered clsoing tag '" + name + "'.");
									throw new InvalidDataException("Invalid timeline format.");

								case State.meta_extensions:
									ProcessClosingMetadataExtensions(name);
									break;
								case State.post_meta:
									LSLogger.LogError("TimelineReader Post Meta state encountered clsoing tag '" + name + "'.");
									throw new InvalidDataException("Invalid timeline format.");

								case State.trk:
									ProcessClosingTrk(name);
									break;
								case State.trkpt:
									ProcessClosingTrkpt(name);
									break;
								case State.trkpt_extensions:
									ProcessClosingTrkptExtensions(name);
									break;
							}
						}
					}
				}
				catch (InvalidDataException e)
				{
					LSLogger.LogError("XMLTimelineReader error while reading timeline at line " + ((IXmlLineInfo)reader).LineNumber + "; " + e.Message);
					throw new InvalidDataException("Error reading timeline - " + e.Message);
				}
			}

			bikeDef = bikeDefinition;
			return timeline;
		}
		#endregion

		#region private_functions
		private void ProcessOpeningStart(string name)
		{
			Log("Start state found opening tag " + name);
			switch (name)
			{
				case "gpx":
					break;
				case "metadata":
					currentState = State.metadata;
					break;
				default:
					LSLogger.LogError("TimelineReader Start state encountered unknown opening tag '" + name + "'.");
					throw new InvalidDataException("Invalid timeline format.");
			}
		}

		private void ProcessOpeningMetadata(string name, XmlReader reader)
		{
			Log("Metadata state found opening tag " + name);
			switch (name)
			{
				case "name":
					timeline.Name = reader.ReadElementContentAsString();
					break;
				case "extensions":
					currentState = State.meta_extensions;
					break;
				default:
					LSLogger.LogError("TimelineReader Metadata state encountered unknown opening tag '" + name + "'.");
					throw new InvalidDataException("Invalid timeline format.");
			}
		}

		private void ProcessOpeningMetadataExtensions(string name, XmlReader reader)
		{
			Log("Metadata Extensions state found opening tag " + name);
			switch (name)
			{
				case "bikename":
					timeline.Bikename = reader.ReadElementContentAsString();
					break;
				case "pedaloffset":
					timeline.Pedaloffset = reader.ReadElementContentAsFloat();
					break;
				default:
					LSLogger.LogError("TimelineReader Metadata Extensions state encountered unknown opening tag '" + name + "'.");
					throw new InvalidDataException("Invalid timeline format.");
			}
		}

		private void ProcessClosingMetadataExtensions(string name)
		{
			Log("Metadata Extensions state found closing tag " + name);
			switch (name)
			{
				case "extensions":
					break;
				case "metadata":
					//get bikeDef
					try
					{
						bikeDefinition = BikeDefReader.ReadBikeDefFromFile(@"BikeDefs/" + timeline.Bikename + ".xml");
					}
					catch (Exception e)
					{
						LSLogger.LogError("TimelineReader unable to load bike definition: " + e.Message);
						throw new InvalidDataException("Invalid bike name.");
					}
					currentState = State.post_meta;
					break;

				default:
					LSLogger.LogError("TimelineReader Metadata Extensions state encountered unknown closing tag '" + name + "'.");
					throw new InvalidDataException("Invalid timeline format.");
			}
		}

		private void ProcessOpeningPostMeta(string name)
		{
			Log("Post Meta state found opening tag " + name);
			switch (name)
			{
				case "trk":
					currentState = State.trk;
					break;
				default:
					LSLogger.LogError("TimelineReader Post Meta state encountered unknown opening tag '" + name + "'.");
					throw new InvalidDataException("Invalid timeline format.");
			}
		}

		private void ProcessOpeningTrk(string name, XmlReader reader)
		{
			Log("Trk state found opening tag " + name);
			switch (name)
			{
				case "trkseg":
					break;
				case "trkpt":
					//start new trackpoint
					currentTrkpt.Clear();

					currentTrkpt.lat = float.Parse(reader.GetAttribute(0), CultureInfo.InvariantCulture);
					currentTrkpt.lon = float.Parse(reader.GetAttribute(1), CultureInfo.InvariantCulture);

					currentState = State.trkpt;
					break;
				default:
					LSLogger.LogError("TimelineReader Trk state encountered unknown opening tag '" + name + "'.");
					throw new InvalidDataException("Invalid timeline format.");
			}
		}

		private void ProcessClosingTrk(string name)
		{
			Log("Trk state found closing tag " + name);
			switch (name)
			{
				case "trkseg":
					isEndReached = true; //end of timeline reached
					break;
				default:
					LSLogger.LogError("TimelineReader Trk state encountered unknown closing tag '" + name + "'.");
					throw new InvalidDataException("Invalid timeline format.");
			}
		}

		private void ProcessOpeningTrkpt(string name, XmlReader reader)
		{
			Log("Trkpt state found opening tag " + name);
			switch (name)
			{
				case "ele":
					currentTrkpt.ele = reader.ReadElementContentAsFloat();
					break;
				case "time":
					currentTrkpt.time = reader.ReadElementContentAsDateTime().ToFileTime();
					break;
				case "extensions":
					currentState = State.trkpt_extensions;
					break;
				default:
					LSLogger.LogError("TimelineReader Trkpt state encountered unknown opening tag '" + name + "'.");
					throw new InvalidDataException("Invalid timeline format.");
			}
		}

		private void ProcessClosingTrkpt(string name)
		{
			Log("Trkpt state found closing tag " + name);
			switch (name)
			{
				case "trkpt":
					if (EXTENDED) Log("Read " + currentTrkpt.ToString());

					//check trackpoint for completeness, add to raw timeline
					if (!currentTrkpt.IsComplete())
					{
						LSLogger.LogError("TimelineReader incomplete trackpoint found.");
						throw new InvalidDataException("Incomplete trackpoint.");
					}
					if (timeline.Time.Count > 0 && currentTrkpt.time <= timeline.Time[timeline.Time.Count - 1])
					{
						LSLogger.LogError("TimelineReader trackpoint time is not ascending.");
						throw new InvalidDataException("Trackpoint time is not ascending.");
					}

					//convert values to in-scene units
					Vector3 adjPos = converter.ConvertGPSToENUScene(new Vector3(currentTrkpt.lat.Value, currentTrkpt.ele.Value, currentTrkpt.lon.Value));

					Vector3 addRot = new Vector3(currentTrkpt.addrotx.Value, currentTrkpt.addroty.Value, currentTrkpt.addrotz.Value);
					Vector3? absRot;
					if (!((currentTrkpt.absrotx ?? currentTrkpt.absroty ?? currentTrkpt.absrotz) == null))
					{
						absRot = new Vector3(currentTrkpt.absrotx.Value, currentTrkpt.absroty.Value, currentTrkpt.absrotz.Value);
					}
					else
					{
						absRot = null;
					}

					if (timeline.Time.Count == 0 && absRot == null)
						throw new InvalidDataException("Missing absolute rotation on first timestamp.");

					Vector3 adjRot = converter.ComputeGlobalAbsRot(previousRotation, addRot, absRot, rotationErrorLimit);
					previousRotation = adjRot;

					float pedalRot = converter.ReconstructGlobalAbsPedalRot(previousPedalRotation, currentTrkpt.pedalrot.Value, currentTrkpt.pedalrotdir.Value);
					previousPedalRotation = pedalRot;

					if (EXTENDED) Log("Converted values: adjPosx " + adjPos.x + "; adjPosy " + adjPos.y + "; adjPosz " + adjPos.z + "; adjRotx " + adjRot.x + "; adjRoty " + adjRot.y + "; adjRotz " + adjRot.z + "; pedalRot " + pedalRot);

					//add trackpoint to timeline
					timeline.Time.Add(currentTrkpt.time.Value);
					timeline.PosX.Add(adjPos.x);
					timeline.PosY.Add(adjPos.y);
					timeline.PosZ.Add(adjPos.z);
					timeline.RotX.Add(adjRot.x);
					timeline.RotY.Add(adjRot.y);
					timeline.RotZ.Add(adjRot.z);
					timeline.Wheelrpm.Add(currentTrkpt.wheelrpm.Value);
					timeline.Steeringrot.Add(currentTrkpt.steeringrot.Value);
					timeline.Pedalrot.Add(pedalRot);
					timeline.Gearfront.Add(currentTrkpt.gearfront.Value);
					timeline.Gearrear.Add(currentTrkpt.gearrear.Value);
					timeline.Brakeright.Add(currentTrkpt.brakeright.Value);
					timeline.Brakeleft.Add(currentTrkpt.brakeleft.Value);
					timeline.Suspfront.Add(currentTrkpt.suspfront.Value);
					timeline.Susprear.Add(currentTrkpt.susprear.Value);
					timeline.Seatpos.Add(currentTrkpt.seatpos.Value);

					currentState = State.trk;
					break;
				default:
					LSLogger.LogError("TimelineReader Trkpt Extensions state encountered unknown closing tag '" + name + "'.");
					throw new InvalidDataException("Invalid timeline format.");
			}
		}

		private void ProcessOpeningTrkptExtensions(string name, XmlReader reader)
		{
			Log("Trkpt Extensions state found opening tag " + name);
			switch (name)
			{
				case "absrotx":
					if (reader.IsEmptyElement) break;
					currentTrkpt.absrotx = ((reader.ReadElementContentAsFloat() % 360) + 360) % 360;
					break;
				case "absroty":
					if (reader.IsEmptyElement) break;
					currentTrkpt.absroty = ((reader.ReadElementContentAsFloat() % 360) + 360) % 360;
					break;
				case "absrotz":
					if (reader.IsEmptyElement) break;
					currentTrkpt.absrotz = ((reader.ReadElementContentAsFloat() % 360) + 360) % 360;
					break;
				case "addrotx":
					currentTrkpt.addrotx = reader.ReadElementContentAsFloat();
					break;
				case "addroty":
					currentTrkpt.addroty = reader.ReadElementContentAsFloat();
					break;
				case "addrotz":
					currentTrkpt.addrotz = reader.ReadElementContentAsFloat();
					break;
				case "wheelrpm":
					currentTrkpt.wheelrpm = reader.ReadElementContentAsFloat();
					break;
				case "steeringrot":
					currentTrkpt.steeringrot = reader.ReadElementContentAsFloat() % 180;
					break;
				case "pedalrot":
					currentTrkpt.pedalrot = ((reader.ReadElementContentAsFloat() % 360) + 360) % 360;
					break;
				case "pedalrotdir":
					int pedalrotdir = reader.ReadElementContentAsInt();
					if (pedalrotdir != -1 && pedalrotdir != 1) throw new InvalidDataException("Invalid pedalrotdir value.");
					currentTrkpt.pedalrotdir = pedalrotdir;
					break;
				case "gearfront":
					int gearfront = reader.ReadElementContentAsInt();
					if (gearfront < 1 || gearfront > bikeDefinition.Frontgears) throw new InvalidDataException("Invalid gearfront value.");
					currentTrkpt.gearfront = gearfront;
					break;
				case "gearrear":
					int gearrear = reader.ReadElementContentAsInt();
					if (gearrear < 1 || gearrear > bikeDefinition.Reargears) throw new InvalidDataException("Invalid gearrear value.");
					currentTrkpt.gearrear = gearrear;
					break;
				case "brakeright":
					float brakeright = reader.ReadElementContentAsFloat();
					if (brakeright < 0 || brakeright > 1) throw new InvalidDataException("Invalid brakeright value.");
					currentTrkpt.brakeright = brakeright;
					break;
				case "brakeleft":
					float brakeleft = reader.ReadElementContentAsFloat();
					if (brakeleft < 0 || brakeleft > 1) throw new InvalidDataException("Invalid brakeleft value.");
					currentTrkpt.brakeleft = brakeleft;
					break;
				case "suspfront":
					float suspfront = reader.ReadElementContentAsFloat();
					if (suspfront < 0 || suspfront > bikeDefinition.Maxfrontsus) throw new InvalidDataException("Invalid suspfront value.");
					currentTrkpt.suspfront = suspfront / bikeDefinition.Maxfrontsus;
					break;
				case "susprear":
					float susprear = reader.ReadElementContentAsFloat();
					if (susprear < 0 || susprear > bikeDefinition.Maxrearsus) throw new InvalidDataException("Invalid susprear value.");
					currentTrkpt.susprear = susprear / bikeDefinition.Maxrearsus;
					break;
				case "seatpos":
					float seatpos = reader.ReadElementContentAsFloat();
					if (seatpos < 0 || seatpos > bikeDefinition.Maxseatpos) throw new InvalidDataException("Invalid seatpos value.");
					currentTrkpt.seatpos = seatpos / bikeDefinition.Maxseatpos;
					break;
				default:
					LSLogger.LogError("TimelineReader Trkpt Extensions state encountered unknown opening tag '" + name + "'.");
					throw new InvalidDataException("Invalid timeline format.");
			}
		}

		private void ProcessClosingTrkptExtensions(string name)
		{
			Log("Trkpt Extensions state found closing tag " + name);
			switch (name)
			{
				case "extensions":
					currentState = State.trkpt;
					break;
				default:
					LSLogger.LogError("TimelineReader Trkpt Extensions state encountered unknown closing tag '" + name + "'.");
					throw new InvalidDataException("Invalid timeline format.");
			}
		}

		private void Log(string message)
		{
			if (DEBUG) LSLogger.Log("XMLTimelineReader", message);
		}
		#endregion
	}
}

