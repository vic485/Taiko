using System.Collections;
using System.IO;
using DataModels;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.Video;

namespace Management
{
    public class BeatmapLoader : MonoBehaviour
    {
        // TODO: Remove these and get from GameManager
        [Header("Debug")]
        public string SongFolder;
        public bool PlayVideo;

        [Space]
        [SerializeField] private AudioSource _audioSource;
        [SerializeField] private Image _image;
        [SerializeField] private VideoPlayer _video;

        private string _basePath;
        private BeatmapData _beatmap;
        
        private IEnumerator Start()
        {
            // TODO: Start loading animation
            _basePath = Path.Combine(Application.streamingAssetsPath, SongFolder);
            _beatmap = JsonUtility.FromJson<BeatmapData>(File.ReadAllText(Path.Combine(_basePath, "mapinfo.json")));
            
            yield return LoadAudio();
            if (PlayVideo)
            {
                yield return LoadVideo();
                _video.Play();
            }
            else
                yield return LoadBackground();
            
            // TODO: Stop loading animation
            _audioSource.Play();
        }

        private IEnumerator LoadAudio()
        {
            using (var musicFile =
                UnityWebRequestMultimedia.GetAudioClip(Path.Combine($"file://{_basePath}", _beatmap.AudioFile),
                    AudioType.WAV))
            {
                yield return musicFile.SendWebRequest();

                if (musicFile.isHttpError || musicFile.isNetworkError)
                {
                    Debug.Log(musicFile.error);
                }
                else
                {
                    _audioSource.clip = DownloadHandlerAudioClip.GetContent(musicFile);
                }
            }
        }

        private IEnumerator LoadBackground()
        {
            using (var backgroundFile =
                UnityWebRequestTexture.GetTexture(Path.Combine(_basePath, _beatmap.BackgroundImage)))
            {
                yield return backgroundFile.SendWebRequest();

                if (backgroundFile.isHttpError || backgroundFile.isNetworkError)
                {
                    Debug.Log(backgroundFile.error);
                }
                else
                {
                    var texture = DownloadHandlerTexture.GetContent(backgroundFile);
                    _image.sprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height),
                        new Vector2(0.5f, 0.5f));
                    _image.preserveAspect = true;
                    _image.gameObject.SetActive(true);
                }
            }
        }

        private IEnumerator LoadVideo()
        {
            _video.url = Path.Combine($"file://{_basePath}", _beatmap.BackgroundVideo);
            _video.Prepare();
            while (!_video.isPrepared)
            {
                yield return null;
            }
        }
    }

}
