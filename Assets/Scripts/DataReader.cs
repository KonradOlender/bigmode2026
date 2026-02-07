using TMPro;
using UnityEngine;

public class DataReader : MonoBehaviour
{
    public DataBase data;
    public TMP_Text label1;
    public TMP_Text label2;
    public TMP_Text label3;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        label1.gameObject.SetActive(false);
        label2.gameObject.SetActive(false);
        label3.gameObject.SetActive(false);

        label1.text = label1.text + data.timeLeft.ToString();
        label2.text = label2.text + data.topSpeed.ToString();
        label3.text = label3.text + data.pingwinKills.ToString();

        if (data.compleated)
        {
            label1.gameObject.SetActive(true);
            label2.gameObject.SetActive(true);
            label3.gameObject.SetActive(true);
        }
    }
}
