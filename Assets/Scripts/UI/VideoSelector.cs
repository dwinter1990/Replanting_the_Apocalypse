using UnityEngine;
using UnityEngine.Video;

public class VideoSelector : MonoBehaviour
{
    [SerializeField] private GameObject videoPlayerPrefab; // Prefab of the video player to instantiate
    [SerializeField] private VideoClip[] videoClips; // Array of video clips to choose from
}
