﻿// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.IO;

namespace Aura.Shared.Util
{
	/// <summary>
	/// All log levels. Also used as bitmask, for hiding.
	/// </summary>
	public enum LogLevel : int
	{
		Info = 0x0001,
		Warning = 0x0002,
		Error = 0x0004,
		Debug = 0x0008,
		Status = 0x0010,
		Exception = 0x0020,
		Unimplemented = 0x0040,
		None = 0x7FFF,
	}

	/// <summary>
	/// Logs messages to command line and file.
	/// </summary>
	public static class Log
	{
		private static string _logFile;

		/// <summary>
		/// Specifies the log levels that shouldn't be displayed
		/// on the command line.
		/// </summary>
		public static LogLevel Hide { get; set; }

		/// <summary>
		/// Sets or returns the directory in which the logs are archived.
		/// If no archive is set, log files will simply be overwritten.
		/// </summary>
		public static string Archive { get; set; }

		/// <summary>
		/// Sets or returns the file to log to. Upon setting the file will
		/// be deleted. If Archive is set, it will be moved to safety first.
		/// </summary>
		public static string LogFile
		{
			get { return _logFile; }
			set
			{
				if (value != null)
				{
					var pathToFile = Path.GetDirectoryName(value);

					if (!Directory.Exists(pathToFile))
						Directory.CreateDirectory(pathToFile);

					if (File.Exists(value))
					{
						if (Archive != null)
						{
							if (!Directory.Exists(Archive))
								Directory.CreateDirectory(Archive);

							var archive = Path.Combine(Archive, File.GetLastWriteTime(value).ToString("yyyy-MM-dd_hh-mm-ss"));
							if (!Directory.Exists(archive))
							{
								Directory.CreateDirectory(archive);
								File.Move(value, Path.Combine(archive, Path.GetFileName(value)));
							}
						}

						File.Delete(value);
					}
				}

				_logFile = value;
			}
		}

		public static void Info(string format, params object[] args)
		{
			WriteLine(LogLevel.Info, format, args);
		}

		public static void Warning(string format, params object[] args)
		{
			WriteLine(LogLevel.Warning, format, args);
		}

		public static void Error(string format, params object[] args)
		{
			WriteLine(LogLevel.Error, format, args);
		}

		public static void Debug(string format, params object[] args)
		{
			WriteLine(LogLevel.Debug, format, args);
		}

		public static void Debug(object obj)
		{
			WriteLine(LogLevel.Debug, obj.ToString());
		}

		public static void Status(string format, params object[] args)
		{
			WriteLine(LogLevel.Status, format, args);
		}

		public static void Exception(Exception ex, string description = null, params object[] args)
		{
			if (description != null)
				WriteLine(LogLevel.Error, description, args);

			WriteLine(LogLevel.Exception, "{2} : {0} in {1}", ex.Source, ex.TargetSite, ex.GetType());
			WriteLine(LogLevel.Exception, ex.Message + Environment.NewLine + ex.StackTrace);
		}

		public static void Unimplemented(string format, params object[] args)
		{
			WriteLine(LogLevel.Unimplemented, format, args);
		}

		public static void WriteLine(LogLevel level, string format, params object[] args)
		{
			Write(level, format + Environment.NewLine, args);
		}

		public static void Write(LogLevel level, string format, params object[] args)
		{
			lock (Console.Out)
			{
				if (!Hide.HasFlag(level))
				{
					switch (level)
					{
						case LogLevel.Info: Console.ForegroundColor = ConsoleColor.White; break;
						case LogLevel.Warning: Console.ForegroundColor = ConsoleColor.Yellow; break;
						case LogLevel.Error: Console.ForegroundColor = ConsoleColor.Red; break;
						case LogLevel.Debug: Console.ForegroundColor = ConsoleColor.Cyan; break;
						case LogLevel.Status: Console.ForegroundColor = ConsoleColor.Green; break;
						case LogLevel.Exception: Console.ForegroundColor = ConsoleColor.DarkRed; break;
						case LogLevel.Unimplemented: Console.ForegroundColor = ConsoleColor.DarkGray; break;
					}

					if (level != LogLevel.None)
						Console.Write("[{0}]", level);

					Console.ForegroundColor = ConsoleColor.Gray;

					if (level != LogLevel.None)
						Console.Write(" - ");

					Console.Write(format, args);
				}

				if (_logFile != null)
				{
					using (var file = new StreamWriter(_logFile, true))
					{
						file.Write(DateTime.Now + " ");
						if (level != LogLevel.None)
							file.Write("[{0}] - ", level);
						file.WriteLine(format.TrimEnd(), args);
						file.Flush();
					}
				}
			}
		}
	}
}