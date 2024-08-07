# AutoLinkLib

當一個UI很複雜的時候常常需要將很多物件拉進去class ref中，
往往會花很多時間有時候還可能拉錯，
有時候更新UI又需要重新拉，
所以就出現這個懶人工具!!!

這是編輯UI時用的小工具，使用方法如下：

![結構圖](https://github.com/user-attachments/assets/a44179a9-14da-4d7f-ac3e-3b5e6ea921fd)

如上圖logoui是ui root，底下有一個gobtn

![logoui圖](https://github.com/user-attachments/assets/219ba53e-6ee8-4516-a0bd-5f6f475ea8e9)

logoui有宣告一個gobtn物件，名稱大小寫須注意!

```c
public class LogoUI : MonoBehaviour
{
    public Button gobtn;

    // Start is called before the first frame update
    void Start()
    {
        gobtn.onClick.AddListener(() =>
        {
            Debug.Log("On Click");
        }
        );
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
```

選擇logoui後，點選menu tools

![AutoLinkLibMenu](https://github.com/user-attachments/assets/92993acd-ef25-4687-970f-7ed2ebbe25a5)

執行AutoLinkLib
最自動對點選物件做名稱和型態比對，成立後自動連結

![自動link](https://github.com/user-attachments/assets/2367235a-b2f6-4af8-a338-d4398b58741b)


