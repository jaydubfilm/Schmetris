using System;
using Sirenix.OdinInspector;

namespace StarSalvager.Factories.Data
{
    //TODO: This is a poor implementation. Come back and revise
    public abstract class RemoteDataBase : IEquatable<RemoteDataBase>
    {
        //public string Name { get; }
        //public float[] Health;

        public abstract bool Equals(RemoteDataBase other);
    }
}

