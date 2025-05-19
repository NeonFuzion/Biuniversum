using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;

public class NetworkManagerUI : MonoBehaviour
{
    [SerializeField] Button serverButton, clientButton, hostButton;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void Awake()
    {
        serverButton.onClick.AddListener(() => { NetworkManager.Singleton.StartServer(); });
        clientButton.onClick.AddListener(() => { NetworkManager.Singleton.StartClient(); });
        hostButton.onClick.AddListener(() => { NetworkManager.Singleton.StartHost(); });
    }
}
