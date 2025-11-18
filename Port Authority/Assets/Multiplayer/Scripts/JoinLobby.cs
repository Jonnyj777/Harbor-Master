using Mirror;
using UnityEngine;
using UnityEngine.UI;

public class JoinLobby : MonoBehaviour
{
    [SerializeField]
    private Button joinButton;

    [SerializeField]
    private InputField codeField;

    private string code;

    private bool isValidCode;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        joinButton.onClick.AddListener(() =>
        {
            if (isValidCode)
            {
                NetworkManager.singleton.networkAddress = code;
                NetworkManager.singleton.StartClient();
            }
        });

        codeField.onValueChanged.AddListener(_ =>
        {
            isValidCode = !string.IsNullOrWhiteSpace(codeField.text);
            code = codeField.text;
        });
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
