
    using Unity.Profiling;
    using UnityEngine;

    public class UpdateHandle
    {
        internal string ObjectName;
        internal Object LinkedObject;
        internal ProfilerMarker Marker;
        internal int UpdateIndex;
        internal int LateUpdateIndex;
        internal int FixedUpdateIndex;

        public UpdateHandle(Object obj)
        {
            ObjectName = obj.GetType().Name;
            LinkedObject = obj;
            Marker = new ProfilerMarker(ObjectName);
            UpdateIndex = -1;
            FixedUpdateIndex = -1;
            LateUpdateIndex = -1;
        }
        
    }
