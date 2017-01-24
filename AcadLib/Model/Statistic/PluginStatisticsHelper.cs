﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PluginStatistic;

namespace AcadLib.Statistic
{
    public static class PluginStatisticsHelper
    {
        public static void PluginStart (CommandStart command)
        {
            Task.Run(() =>
            {
                try
                {                    
                    Writer.WriteRunToDB("AutoCAD",
                                command.Plugin, command.CommandName,
                                FileVersionInfo.GetVersionInfo(command.Assembly.Location).ProductVersion,
                                command.Doc);
                }
                catch (Exception ex)
                {
                    Logger.Log.Error(ex, "PluginStatisticsHelper.PluginStart");
                }
            });
        }

        public static void AddStatistic ()
        {
            try
            {
                var caller = new StackTrace().GetFrame(1).GetMethod();
                PluginStart(CommandStart.GetCallerCommand(caller));
            }
            catch(Exception ex)
            {
                Logger.Log.Error(ex, "PluginStatisticsHelper.AddStatistic");
            }
        }
    }
}
