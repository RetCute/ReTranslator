using System.Windows.Media;
using System.Windows;
using System;

namespace ReTranslator.Utilities
{
    public static class Logger
    {
        public static event Action<string, Brush> LogAdded;
        
        public static void Info(string message) => AddLog(message, "Info");
        public static void Success(string message) => AddLog(message, "Success");
        public static void Warning(string message) => AddLog(message, "Warning");
        public static void Error(string message) => AddLog(message, "Error");
        
        private static void AddLog(string message, string logLevel)
        {
            Brush brush = GetLevelBrush(logLevel);
            message = $"[{logLevel}] {message}";
            LogAdded?.Invoke(message, brush);
        }
        
        private static Brush GetLevelBrush(string key)
        {
            if (key != null && Application.Current.Resources.Contains(key))
            {
                return (Brush)Application.Current.Resources[key];
            }
            
            return Brushes.Gray; // 默认灰色
        }
    }
}