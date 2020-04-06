public interface IMegacoolPreviewData {
    IMegacoolPreviewFrame GetNextFrame();
    int GetNumberOfFrames();
    void Release();
}
