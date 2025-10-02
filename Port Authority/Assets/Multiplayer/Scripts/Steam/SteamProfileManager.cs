using Steamworks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Threading.Tasks;

public class SteamProfileManager : MonoBehaviour
{
    public RawImage profilePicture;
    public TextMeshProUGUI playerName;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    async void Start()
    {
        playerName.text = SteamClient.Name;
        profilePicture.texture = await GetTextureFromId(SteamClient.SteamId);
    }

    public static async Task<Texture2D> GetTextureFromId(SteamId id)
    {
        Steamworks.Data.Image? img = await SteamFriends.GetLargeAvatarAsync(id);
        return TextureExtract(img.Value);
    }

    public static Texture2D TextureExtract(Steamworks.Data.Image img)
    {
        Texture2D texture = new Texture2D((int)img.Width, (int)img.Height);

        for (int x = 0; x < img.Width; x++)
        {
            for (int y = 0; y < img.Height; y++)
            {
                Steamworks.Data.Color pixelData = img.GetPixel(x, y);
                Color pixelColor = new Color(pixelData.r / 255.0f, pixelData.g / 255.0f, pixelData.b / 255.0f, pixelData.a / 255.0f);
                texture.SetPixel(x, (int)img.Height - y, pixelColor);
            }
        }

        texture.Apply();
        return texture;
    }

    

    // Update is called once per frame
    void Update()
    {

    }
}
