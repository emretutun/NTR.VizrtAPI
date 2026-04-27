using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NTR.Core.Entities;
using NTR.Core.Interfaces;
using NTR.Infrastructure.Tcp;

namespace NTR.Infrastructure.Vizrt
{
    public class VizrtEngineClient : IVizrtEngine
    {
        private readonly IVizrtConnection _connection;
        private int _uniqueResponseCode = 0;

        public int Id { get; }
        public string Name { get; }

        public bool IsConnected => _connection.IsConnected;

        public string CurrentScene_Front { get; private set; } = "";
        public string CurrentScene_Middle { get; private set; } = "";
        public string CurrentScene_Back { get; private set; } = "";

        public VizrtEngineClient(int id, string name)
        {
            Id = id;
            Name = name;
            _connection = new VizrtTcpConnection();
        }

        public bool Connect(string ip)
        {
            if (ip.Contains(":"))
            {
                string[] parts = ip.Split(':');
                _connection.IP = parts[0];
                _connection.Port = int.Parse(parts[1]);
            }
            else
            {
                _connection.IP = ip;
                _connection.Port = 6100;
            }

            _connection.ParentName = Name;
            return _connection.Connect();
        }

        public bool Disconnect()
        {
            return _connection.Disconnect();
        }

        public CommandResult Send(string command)
        {
            bool result = _connection.Send($"-1 {command}");
            return result
                ? CommandResult.Ok($"Command sent: {command}")
                : CommandResult.Fail("Not connected or send failed");
        }

        private int SendPack(string command)
        {
            if (!IsConnected) return -1;
            _connection.Send($"{_uniqueResponseCode} {command}");
            return _uniqueResponseCode++;
        }

        public void Play(string scene, string animName)
        {
            Send($"RENDERER*STAGE*DIRECTOR*${animName} START");
        }

        public void ReversePlay(string scene, string animName)
        {
            Send($"RENDERER*STAGE*DIRECTOR*${animName} START REVERSE");
        }

        public void SetObjectText(string scene, string objectName, string text)
        {
            Send($"RENDERER*TREE*${objectName}*GEOM*TEXT SET {text}");
        }

        public void Visibility(string scene, string objectName, bool state)
        {
            Send($"RENDERER*TREE*${objectName}*ACTIVE SET {(state ? "1" : "0")}");
        }

        public void LoadScene(string scene)
        {
            CurrentScene_Middle = scene;
            Send($"RENDERER SET_OBJECT SCENE*{scene}");
        }

        public void LoadScene_ToFront(string scene)
        {
            CurrentScene_Front = scene;
            Send($"RENDERER*FRONT_LAYER SET_OBJECT SCENE*{scene}");
        }

        public void LoadScene_ToBack(string scene)
        {
            CurrentScene_Back = scene;
            Send($"RENDERER*BACK_LAYER SET_OBJECT SCENE*{scene}");
        }

        public void StageToStart(string layer)
        {
            Send($"{layer}*STAGE TO_START");
        }

        public void FullCleanup()
        {
            Send("SCENE CLEANUP");
            Send("GEOM CLEANUP");
            Send("IMAGE CLEANUP");
            Send("FONT CLEANUP");
            Send("BASE_FONT CLEANUP");
            Send("RENDERER*FRONT_LAYER SET_OBJECT");
            Send("RENDERER SET_OBJECT");
            Send("RENDERER*BACK_LAYER SET_OBJECT");

            CurrentScene_Front = "";
            CurrentScene_Middle = "";
            CurrentScene_Back = "";
        }

        public VizrtEngine GetStatus()
        {
            return new VizrtEngine
            {
                Id = Id,
                Name = Name,
                IP = _connection.IP,
                Port = _connection.Port,
                IsConnected = IsConnected,
                CurrentScene_Front = CurrentScene_Front,
                CurrentScene_Middle = CurrentScene_Middle,
                CurrentScene_Back = CurrentScene_Back
            };
        }
    }
}