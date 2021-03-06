﻿using System.Collections;
using System.IO;
using AudioHelpers;
using DataModels;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.Video;

namespace Management
{
    public class BeatmapLoader : MonoBehaviour
    {
        // TODO: Get from GameManager
        public string folderName;
        
        [SerializeField] private Image backgroundImage;
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private VideoPlayer videoPlayer;
        
        private string _beatmapFolder;
        private BeatmapInfo beatmap;

        private IEnumerator Start()
        {
            _beatmapFolder = Path.Combine(Application.streamingAssetsPath, "songs", folderName);
            LoadBeatmap();

            yield return LoadAudio();
            
            if (!string.IsNullOrWhiteSpace(beatmap.VideoFile))
            {
                yield return LoadVideo();
                videoPlayer.Play();
            }
            else
            {
                yield return LoadBackgroundImage();
            }
            
            audioSource.Play();
        }

        private void LoadBeatmap()
        {
            var path = Path.Combine(_beatmapFolder, "mapinfo.tko");
            beatmap = JsonConvert.DeserializeObject<BeatmapInfo>(File.ReadAllText(path));
        }

        private IEnumerator LoadBackgroundImage()
        {
            using (var imageRequest =
                UnityWebRequestTexture.GetTexture(Path.Combine($"file://{_beatmapFolder}", beatmap.BackgroundImage)))
            {
                yield return imageRequest.SendWebRequest();

                if (imageRequest.isHttpError || imageRequest.isNetworkError)
                {
                    Debug.Log(imageRequest.error);
                }
                else
                {
                    var texture = DownloadHandlerTexture.GetContent(imageRequest);
                    backgroundImage.sprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height),
                        new Vector2(0.5f, 0.5f));
                    backgroundImage.preserveAspect = true;
                    backgroundImage.gameObject.SetActive(true);
                }
            }
        }

        private IEnumerator LoadAudio()
        {
            /*using (var musicRequest =
                UnityWebRequestMultimedia.GetAudioClip(Path.Combine($"file://{_beatmapFolder}", beatmap.AudioFile),
                    AudioType.WAV))
            {
                yield return musicRequest.SendWebRequest();
                
                if (musicRequest.isHttpError || musicRequest.isNetworkError)
                {
                    Debug.Log(musicRequest.error);
                }
                else
                {
                    audioSource.clip = DownloadHandlerAudioClip.GetContent(musicRequest);
                }
            }*/
            var filePath = Path.Combine(_beatmapFolder, beatmap.AudioFile);
            var extension = Path.GetExtension(beatmap.AudioFile);
            switch (extension)
            {
                case ".mp3":
                    // TODO: Actually run this asynchronously?
                    audioSource.clip = Mp3Util.GetAudio(filePath);
                    break;
                case ".wav":
                    yield return UnityAudioLoader(filePath, AudioType.WAV);
                    break;
                default:
                    Debug.LogError($"{extension} file type is not yet supported.");
                    break;
            }
            yield return null;
        }

        private IEnumerator LoadVideo()
        {
            videoPlayer.url = Path.Combine(_beatmapFolder, beatmap.VideoFile);
            videoPlayer.Prepare();
            while (!videoPlayer.isPrepared)
                yield return null;
        }

        private IEnumerator UnityAudioLoader(string filePath, AudioType audioType)
        {
            using (var musicRequest = UnityWebRequestMultimedia.GetAudioClip($"file://{filePath}", audioType))
            {
                yield return musicRequest.SendWebRequest();
                
                if (musicRequest.isHttpError || musicRequest.isNetworkError)
                {
                    Debug.Log(musicRequest.error);
                }
                else
                {
                    audioSource.clip = DownloadHandlerAudioClip.GetContent(musicRequest);
                }
            }
        }
    }

}
