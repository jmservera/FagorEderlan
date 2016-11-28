using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace FileSender
{
    public class Logger : TraceListener
    {
        private const string EventSource = "Ederlan Gateway";
        private const string LogName = "Application";

        private int simulatorCounter = 0;

        private  void CheckSource(string source)
        {
            if (!EventLog.SourceExists(source))
            {
                EventLog.CreateEventSource(source, Logger.LogName);
            }
        }

        public void WriteEntry(string message, EventLogEntryType entryType)
        {
            try
            {
                this.CheckSource(Logger.EventSource);
                EventLog.WriteEntry(Logger.EventSource, message, entryType);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public void WriteEntry(string message, EventLogEntryType entryType, int eventId)
        {
            try
            {
                this.CheckSource(Logger.EventSource);
                EventLog.WriteEntry(Logger.EventSource, message, entryType, eventId);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public void WriteErrorEntry(string description)
        {
            this.WriteEntry(description, EventLogEntryType.Error);
        }

        public void WriteInformationEntry(string description)
        {
            this.WriteEntry(description, EventLogEntryType.Information);
        }

        public void WriteWarningEntry(string description)
        {
            this.WriteEntry(description, EventLogEntryType.Warning);
        }

        public void WriteSuccessEntry(string description)
        {
            this.WriteEntry(description, EventLogEntryType.SuccessAudit);
        }

        public void WriteFailureEntry(string description)
        {
            this.WriteEntry(description, EventLogEntryType.FailureAudit);
        }

        public void StartSimulator()
        {
            Trace.Fail("message");
            Trace.Fail("message", "detailsMessage");
            Trace.TraceError("message");
            Trace.TraceError("{0} {1}", "param1", "param2");
            Trace.TraceInformation("message");
            Trace.TraceInformation("message", new string[] { "param1", "param2" });
            Trace.TraceWarning("message");
            Trace.TraceWarning("message", new string[] { "param1", "param2" });
            Trace.Write("object");
            Trace.Write("message");
            Trace.Write("object", "category");
            Trace.Write("message", "category");
            Trace.WriteIf(true, "object");
            Trace.WriteIf(true, "message");
            Trace.WriteIf(true, "object", "category");
            Trace.WriteIf(true, "message", "category");
            Trace.WriteLine("object");
            Trace.WriteLine("message");
            Trace.WriteLine("object", "category");
            Trace.WriteLine("message", "category");
            Trace.WriteLineIf(true, "object");
            Trace.WriteLineIf(true, "message");
            Trace.WriteLineIf(true, "object", "category");
            Trace.WriteLineIf(true, "message", "category");

            //Timer timer = new Timer(5000);
            //timer.Elapsed += this.TimerElapsed;
            //timer.Start();
        }

        private void TimerElapsed(object sender, ElapsedEventArgs e)
        {
            this.simulatorCounter++;
            if (simulatorCounter % 2 == 0)
                this.WriteInformationEntry($"Test information entry. Counter: {this.simulatorCounter}");
            else
                this.WriteErrorEntry($"Test error entry. Counter: {this.simulatorCounter}");
        }

        // Fail("message");
        public override void Fail(string message)
        {
            //base.Fail(message);
            this.WriteEntry(message, EventLogEntryType.FailureAudit);
        }

        // Fail("message", "detailsMessage");
        public override void Fail(string message, string detailMessage)
        {
            //base.Fail(message, detailMessage);
            string myMessage = message;
            if (detailMessage != null)
                myMessage += $"{Environment.NewLine}{Environment.NewLine}Details: {detailMessage}";
            this.WriteEntry(myMessage, EventLogEntryType.FailureAudit);
        }

        // TraceError("message", new string[] { "param1", "param2" })
        // TraceInformation("message", new string[] { "param1", "param2" });
        // TraceWarning("message", new string[] { "param1", "param2" });
        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string format, params object[] args)
        {
            //base.TraceEvent(eventCache, source, eventType, id, format, args);
            string message = string.Format(format, args);
            EventLogEntryType type = EventLogEntryType.Information;
            if (eventType == TraceEventType.Error || eventType == TraceEventType.Critical)
                type = EventLogEntryType.Error;
            else if (eventType == TraceEventType.Warning)
                type = EventLogEntryType.Warning;
            this.WriteEntry(message, type, id);
        }

        // TraceError("message")
        // TraceInformation("message");
        // TraceWarning("message");
        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string message)
        {
            //base.TraceEvent(eventCache, source, eventType, id, message);
            EventLogEntryType type = EventLogEntryType.Information;
            if (eventType == TraceEventType.Error || eventType == TraceEventType.Critical)
                type = EventLogEntryType.Error;
            else if (eventType == TraceEventType.Warning)
                type = EventLogEntryType.Warning;
            this.WriteEntry(message, type, id);
        }

        public override void Write(string message)
        {
            this.WriteInformationEntry(message);
        }

        public override void WriteLine(string message)
        {
            this.WriteInformationEntry(message);
        }
    }
}
