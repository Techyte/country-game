using Riptide;
using UnityEngine;

namespace CountryGame.Multiplayer
{
    public static class MessageExtensions
    {
        #region Vector2

        public static Message AddVector2(this Message message, Vector2 value) => Add(message, value);

        public static Message Add(this Message message, Vector2 value)
        {
            message.AddFloat(value.x);
            message.AddFloat(value.y);
            return message;
        }

        public static Vector2 GetVector2(this Message message)
        {
            return new Vector2(message.GetFloat(), message.GetFloat());
        }

        #endregion

        #region Vector3

        public static Message AddVector3(this Message message, Vector3 value) => Add(message, value);

        public static Message Add(this Message message, Vector3 value)
        {
            message.AddFloat(value.x);
            message.AddFloat(value.y);
            message.AddFloat(value.z);
            return message;
        }

        public static Vector3 GetVector3(this Message message)
        {
            return new Vector3(message.GetFloat(), message.GetFloat(), message.GetFloat());
        }

        #endregion

        #region Quaternion

        public static Message AddQuaternion(this Message message, Quaternion value) => Add(message, value);

        public static Message Add(this Message message, Quaternion value)
        {
            message.AddFloat(value.x);
            message.AddFloat(value.y);
            message.AddFloat(value.z);
            message.AddFloat(value.w);
            return message;
        }

        public static Quaternion GetQuaternion(this Message message)
        {
            return new Quaternion(message.GetFloat(), message.GetFloat(), message.GetFloat(), message.GetFloat());
        }

        #endregion
    }
}