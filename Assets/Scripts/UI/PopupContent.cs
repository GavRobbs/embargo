public struct PopupContent 
{
    public string title;
    public string status;
    public int hp;
    public int max_hp;
    public string emplacement_name;
    public int emplacement_level;
    public bool short_version;

    public PopupContent(string t, int h, int mh, string en, int el)
    {
        title = t;
        hp = h;
        max_hp = mh;
        emplacement_name = en;
        emplacement_level = el;
        short_version = false;
        status = "";
    }

    public PopupContent(string t, string s)
    {
        title = t;
        status = s;
        hp = 0;
        max_hp = 0;
        emplacement_name = "";
        emplacement_level = 0;
        short_version = true;
    }
}
