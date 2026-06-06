using NAudio.CoreAudioApi;
using System;
using System.Collections.Generic;
using Utilities;

namespace EddiSpeechService
{
    public class AudioDevice
    {
        public string Name { get; set; }
        public string Id { get; set; }
    }

    public static class AudioDeviceService
    {
        public static List<AudioDevice> GetAudioDevices()
        {
            var list = new List<AudioDevice>();
            try
            {
                var enumerator = new MMDeviceEnumerator();
                var devices = enumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active);
                foreach (var device in devices)
                {
                    list.Add(new AudioDevice
                    {
                        Name = device.FriendlyName,
                        Id = device.ID
                    });
                }
            }
            catch (Exception ex)
            {
                Logging.Error("Failed to list audio devices", ex);
            }
            return list;
        }
    }
}
