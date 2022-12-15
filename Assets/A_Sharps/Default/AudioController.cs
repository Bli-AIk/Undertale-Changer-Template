using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ���ڶ���� ������Ƶ
/// </summary>
public class AudioController : MonoBehaviour
{
    public static AudioController instance;
    [Header("������صĶ�������")]
    public int count;
    GameObject fx;
    Queue<GameObject> availbleFx = new Queue<GameObject>();

    private void Awake()
    {
        instance = this;


        fx = new GameObject();
        AudioPlayer audioPlayer = fx.AddComponent<AudioPlayer>();
        audioPlayer.audioSource = fx.AddComponent<AudioSource>();
       
        fx.gameObject.name = "FX Source";
        fx.SetActive(false);
        FillPool();
    }
    public void GetFx(AudioClip list, float volume = 1)
    {
        GameObject fx = GetFromPool();
        fx.GetComponent<AudioSource>().volume = volume;
        //AudioPlayer�����࣡������unity�Դ���
        fx.GetComponent<AudioPlayer>().Playing(list);
    }

    public void GetFx(int fxNum,List<AudioClip> list,float volume = 1)
    {
        GameObject fx = GetFromPool();
        fx.GetComponent<AudioSource>().volume = volume;
        //AudioPlayer�����࣡������unity�Դ���
        fx.GetComponent<AudioPlayer>().Playing(list[fxNum]);
    }
    /*    
    private void Update()
    {
        if (Input.GetKeyDown("s")) 
        {
            for (int i = 0; i < 500; i++)//��Ĥը�Ѽ�
            {

                GetFx(Random.Range(0, 5));
            }
        }
    }
    */
    //-----����ز���-----

    /// <summary>
    /// ��ʼ��/�������
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
    /// ���ض����
    /// </summary>
    public void ReturnPool(GameObject gameObject)
    {
        gameObject.SetActive(false);
        gameObject.transform.SetParent(transform);
        availbleFx.Enqueue(gameObject);
    }
    /// <summary>
    /// ϲ����� FX
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
