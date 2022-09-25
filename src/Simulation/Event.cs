/* Copyright (C) 2019-2022 Junruoyu Zheng. Home page: https://junruoyu-zheng.gitee.io/ligral

   Distributed under MIT license.
   See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/

using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics.LinearAlgebra;

namespace Ligral.Simulation
{
    public class Event
    {
        public double PreviousValue = 0;
        public double CurrentValue;
        public double Threshold = 0.999;
        public bool Detected
        {
            // from non-zero to zero or cross zero line
            get =>(PreviousValue!=0 && CurrentValue==0) || PreviousValue*CurrentValue<0;
        }
        public bool Hitted
        {
            get 
            {
                bool hitted = Detected && ScalingValue>Threshold;
                if (hitted) logger.Info($"Event {Name} hitted");
                return hitted;
            }
        }
        public double ScalingValue
        {
            // if not active then no scaling
            // if actual zero then hit the event, thus no scaling
            get => ((!Detected)||CurrentValue==0) ? 1 : 1+PreviousValue/(CurrentValue-PreviousValue);
        }
        private static Logger logger = new Logger("Event");
        public static List<Event> EventPool = new List<Event>();
        public static Dictionary<string, EventHandle> EventHandles = new Dictionary<string, EventHandle>();
        public string Name;
        public static Event CreateEvent(string name)
        {
            name = name??$"Event{EventPool.Count}";
            if (EventPool.Exists(Event => Event.Name == name))
            {
                throw logger.Error(new LigralException($"Event {name} has already existed."));
            }
            else
            {
                Event Event = new Event(name);
                EventPool.Add(Event);
                return Event;
            }
        }
        public static EventHandle CreateEvent(string name, int rowNo, int colNo)
        {
            name = name??$"Event{EventPool.Count}";
            if (EventHandles.ContainsKey(name))
            {
                throw logger.Error(new LigralException($"Event handle {name} has already existed."));
            }
            else
            {
                var handle = new EventHandle(name, rowNo, colNo);
                EventHandles.Add(name, handle);
                return handle;
            }
        }
        public static string GetNames()
        {
            return $"[{string.Join(", ", EventPool.Select((Event, index)=>Event.Name))}]";
        }
        private Event(string name) 
        {
            Name = name;
        }
    }
    public class EventHandle : Handle<Event>
    {
        public EventHandle(string name, int rowNo, int colNo) : 
        base(name, rowNo, colNo, name => Event.CreateEvent(name)) {}

        public void SetCurrentValue(Matrix<double> current)
        {
            SetSignal(current, (ev, current) => ev.CurrentValue = current);
        }
    }
}