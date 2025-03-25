using System;
using UnityEngine;
// ReSharper disable UnusedMember.Global

namespace UCT.Battle
{
    public readonly struct InitialTransform : IEquatable<InitialTransform>
    {
        public Vector3? Position { get; }
        public Quaternion? Rotation { get; }
        public Vector3? Scale { get; }

        public InitialTransform(Vector3 position)
        {
            Position = position;
            Rotation = null;
            Scale = null;
        }

        public InitialTransform(Vector3 position, Quaternion rotation)
        {
            Position = position;
            Rotation = rotation;
            Scale = null;
        }

        public InitialTransform(Vector3 position, Vector3 scale)
        {
            Position = position;
            Rotation = null;
            Scale = scale;
        }

        public InitialTransform(Vector3 position, Quaternion rotation, Vector3 scale)
        {
            Position = position;
            Rotation = rotation;
            Scale = scale;
        }

        public bool Equals(InitialTransform other)
        {
            return Position.Equals(other.Position) && Rotation.Equals(other.Rotation) && Scale.Equals(other.Scale);
        }

        public override bool Equals(object obj)
        {
            return obj is InitialTransform other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Position, Rotation, Scale);
        }
    }
}