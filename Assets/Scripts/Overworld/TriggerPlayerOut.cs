using UnityEngine;

/// <summary>
/// V0.1.1�¼�
/// ���ڴ���������OW������ҽ���/�뿪ʱ��ִ�д���/���Ŷ�����
/// �ɶ෽��������Ҳ�
/// </summary>
public class TriggerPlayerOut : MonoBehaviour
{
    public float volume = 1;
    public float pitch = 1;

    [Header("���Ķ����Ĳ���ֵ��")]
    public string changeBool;

    [Header("ȥ�������ⷶΧִ�ж���")]
    public bool banTrigger;

    [Header("���ϸ�����Ϊָ������ʱȥ��ʧЧ")]
    public string notBanSceneSp;

    [Header("�������뿪��Χ/���뷶Χʱִ��")]
    public bool isEnter;

    private Animator animator;
    private bool triggered = false;

    [Header("���ϸ�����Ϊָ������ʱ��������sceneBool��true")]
    public string sceneSp;

    public string sceneBool = "SceneSp";

    [Header("��electricOpen��ʱ��electricOpen")]
    public bool electricOpen;

    private void Start()
    {
        animator = GetComponent<Animator>();

        if (sceneSp == MainControl.instance.PlayerControl.lastScene)
        {
            animator.SetBool(sceneBool, true);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (banTrigger && notBanSceneSp != MainControl.instance.PlayerControl.lastScene)
            return;
        if (!triggered && isEnter && collision.CompareTag("Player"))
        {
            animator.SetBool(changeBool, true);
            triggered = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (banTrigger && notBanSceneSp != MainControl.instance.PlayerControl.lastScene)
            return;
        if (!triggered && !isEnter && collision.CompareTag("Player"))
        {
            animator.SetBool(changeBool, true);
            triggered = true;
        }
    }

    /*
    //���������õ�

    void SetNoActive(string name)
    {
        transform.Find(name).gameObject.SetActive(false);
    }
	void SetActive(string name)
    {
        transform.Find(name).gameObject.SetActive(true);
    }
	void ChangePitch(float newPitch)
	{
		pitch = newPitch;
	}
    void ChangeVolume(float newVolume)
    {
        volume = newVolume;
    }
    void UseMixer(int boolean)
    {
        if (Convert.ToBoolean(boolean))
            mixer = Resources.Load<AudioMixerGroup>("AudioMixer/TrueLabAudioMixer");
        else mixer = null;
    }
    void PlayFx(int i)
    {
        AudioController.instance.GetFx(i, MainControl.instance.AudioControl.fxClipUI, volume, pitch, mixer);
    }
    void PlayFxBattle(int i)
    {
        AudioController.instance.GetFx(i, MainControl.instance.AudioControl.fxClipBattle, volume, pitch, mixer);
    }
    */
}