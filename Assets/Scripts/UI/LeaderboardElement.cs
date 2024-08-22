using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LeaderboardElement : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI rankText;

    [SerializeField] private Image image;
    [SerializeField] private Sprite topSprite;
    
    
    public void Setup(string name, string score, int rank)
    {
        nameText.text = name;
        scoreText.text = score + " GDP";
        // if rank is not 10 add a 0 before it
        rankText.text = rank < 10 ? "0" + rank : rank.ToString();
        
        if (rank == 1)
        {
            image.sprite = topSprite;
        }
    }
}