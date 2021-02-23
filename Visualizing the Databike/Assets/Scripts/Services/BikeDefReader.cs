using VisualizingTheDatabike.DataModel;
using SmidtFramework.LoggingSystem;
using System.IO;
using System.Xml;

namespace VisualizingTheDatabike.Services
{
	/// <summary>
	/// Class for reading a bike definition file.
	/// </summary>
	public static class BikeDefReader
	{
		/// <summary>
		/// Reads a bike definition file from filepath and parses it to a BikeDefinition. Throws an InvalidDataException on error.
		/// </summary>
		/// <param name="filepath">Path to the file.</param>
		/// <returns></returns>
		public static BikeDefinition ReadBikeDefFromFile(string filepath)
		{
			BikeDefinition bikeDefinition = new BikeDefinition();
			const int attrCount = 8;
			int[] countCheck = new int[attrCount]; //to check if all properties present

			using (XmlReader reader = XmlReader.Create(File.OpenRead(filepath)))
			{
				while (reader.Read())
				{
					if (reader.NodeType == XmlNodeType.Element)
					{
						//Logger.Log("BikeDefReader", "start element " + reader.Name + ", " + reader.Value + ", attr: " + reader.AttributeCount);
						switch (reader.Name.ToLowerInvariant())
						{
							case "bikedef":
								//LSLogger.Log("BikeDefReader", "bikedef tag found.");
								break;
							case "bikename":
								bikeDefinition.Name = reader.ReadElementContentAsString();
								countCheck[0]++;
								break;
							case "prefabpath":
								bikeDefinition.Prefabpath = reader.ReadElementContentAsString();
								countCheck[1]++;
								break;
							case "frontgears":
								bikeDefinition.Frontgears = reader.ReadElementContentAsInt();
								countCheck[2]++;
								break;
							case "reargears":
								bikeDefinition.Reargears = reader.ReadElementContentAsInt();
								countCheck[3]++;
								break;
							case "maxfrontsus":
								bikeDefinition.Maxfrontsus = reader.ReadElementContentAsFloat();
								countCheck[4]++;
								break;
							case "maxrearsus":
								bikeDefinition.Maxrearsus = reader.ReadElementContentAsFloat();
								countCheck[5]++;
								break;
							case "maxseatpos":
								bikeDefinition.Maxseatpos = reader.ReadElementContentAsFloat();
								countCheck[6]++;
								break;
							case "frontbrake":
								string content = reader.ReadElementContentAsString();
								if (content == "left") bikeDefinition.FrontBrake = 0;
								else if (content == "right") bikeDefinition.FrontBrake = 1;
								else
								{
									LSLogger.LogError("BikeDefReader frontbrake invalid value '" + content + "'.");
									throw new InvalidDataException("BikeDefReader frontbrake invalid value '" + content + "'.");
								}
								countCheck[7]++;
								break;
							default:
								LSLogger.LogError("BikeDefReader encountered unknown opening tag '" + reader.Name + "'.");
								throw new InvalidDataException("Invalid bikedef format - invalid opening tag.");
						}
					}
					else if (reader.NodeType == XmlNodeType.EndElement)
					{
						if (reader.Name.ToLowerInvariant() == "bikedef")
						{
							for(int i = 0; i < attrCount; i++)
							{
								if (countCheck[i] != 1) throw new InvalidDataException("Invalid bikedef format - missing properties.");
							}
						}
						else
						{
							LSLogger.LogError("BikeDefReader encountered unknown closing tag '" + reader.Name + "'.");
							throw new InvalidDataException("Invalid bikedef format - invalid closing tag.");
						}
					}
				}
			}

			return bikeDefinition;
		}
	}
}
