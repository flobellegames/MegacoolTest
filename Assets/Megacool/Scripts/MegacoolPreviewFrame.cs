using UnityEngine;

public interface IMegacoolPreviewFrame {
    int GetDelayMs();

    bool LoadToTexture(Texture2D texture);

    void Release();
}
