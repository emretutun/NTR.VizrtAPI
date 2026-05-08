using System.Threading.Tasks;
using NTR.Core.Entities;

namespace NTR.Core.Interfaces
{
    public interface IVizrtEngine
    {
        int Id { get; }
        string Name { get; }
        bool IsConnected { get; }

        bool Connect(string ip);
        bool Disconnect();
        CommandResult Send(string command);

        void Play(string scene, string animName);

        /// <summary>
        /// Animasyonu oynatır ve motordan ACK bekler.
        /// ACK gelmezse fallbackDelayMs kadar bekler (fire-and-forget fallback).
        /// </summary>
        Task PlayAndWaitAsync(string scene, string animName, int fallbackDelayMs);

        void SetObjectText(string scene, string objectName, string text);
        void Visibility(string scene, string objectName, bool state);
        void LoadScene(string scene);
        void StageToStart(string layer);
        void FullCleanup();
        VizrtEngine GetStatus();
    }
}