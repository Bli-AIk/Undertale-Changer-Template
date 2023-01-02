using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
/// <summary>
/// 基于对象池 控制音频
/// </summary>
public class AudioController : MonoBehaviour
{
    public static AudioController instance;
    [Header("填充对象池的对象数量")]
    public int count;

    GameObject fx;
    Queue<GameObject> availbleFx = new Queue<GameObject>();
    public AudioSource audioSource;
    private void Awake()
    {
        instance = this;


        fx = new GameObject();
        AudioPlayer audioPlayer = fx.AddComponent<AudioPlayer>();
        audioPlayer.audioSource = fx.AddComponent<AudioSource>();
       
        fx.gameObject.name = "FX Source";
        fx.SetActive(false);
        FillPool();
        audioSource = GetComponent<AudioSource>();



    }
    public void GetFx(AudioClip list, float volume = 0.5f, float pitch = 1, AudioMixerGroup audioMixerGroup = null)
    {
        GameObject fx = GetFromPool();
        fx.GetComponent<AudioSource>().volume = volume;
        fx.GetComponent<AudioSource>().outputAudioMixerGroup = audioMixerGroup;
        //AudioPlayer是字类！！不是unity自带的
        fx.GetComponent<AudioPlayer>().Playing(list);
    }

    public void GetFx(int fxNum, List<AudioClip> list, float volume = 0.5f, float pitch = 1, AudioMixerGroup audioMixerGroup = null)
    {
        if (fxNum < 0)
            return;
        GameObject fx = GetFromPool();
        fx.GetComponent<AudioSource>().volume = volume;
        fx.GetComponent<AudioSource>().pitch = pitch;
        fx.GetComponent<AudioSource>().outputAudioMixerGroup = audioMixerGroup;
        //AudioPlayer是字类！！不是unity自带的
        fx.GetComponent<AudioPlayer>().Playing(list[fxNum]);
    }

    public IEnumerator LayerGetFx(float time, int fxNum, List<AudioClip> list, float volume = 0.5f, float pitch = 1, UnityEngine.Audio.AudioMixerGroup audioMixerGroup = null)
    {
        yield return new WaitForSeconds(time);
        GetFx(fxNum, list, volume, pitch, audioMixerGroup);
    }
    /*    
    private void Update()
    {
        if (Input.GetKeyDown("s")) 
        {
            for (int i = 0; i < 500; i++)//耳膜炸裂级
            {

                GetFx(Random.Range(0, 5));
            }
        }
    }
    */
    //-----对象池部分-----

    /// <summary>
    /// 初始化/填充对象池
    /// </summary>
    public void FillPool()
    {
        for (int i = 0; i < count; i++)
        { 
            var newObj = Instantiate(fx, transform);
            ReturnPool(newObj);
        }
    }
    /// <summary>
    /// 返回对象池
    /// </summary>
    public void ReturnPool(GameObject gameObject)
    {
        gameObject.SetActive(false);
        gameObject.transform.SetParent(transform);
        availbleFx.Enqueue(gameObject);
    }
    /// <summary>
    /// 喜提对象 FX
    /// </summary>
    public GameObject GetFromPool()
    {
        if (availbleFx.Count == 0)
            FillPool();
        
        var fx = availbleFx.Dequeue();

        fx.SetActive(true);
        return fx;
    }
}
