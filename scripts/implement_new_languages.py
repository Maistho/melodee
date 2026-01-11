import xml.etree.ElementTree as ET
import os

languages = {
    "nl-NL": "Dutch",
    "pl-PL": "Polish",
    "tr-TR": "Turkish",
    "id-ID": "Indonesian",
    "ko-KR": "Korean",
    "vi-VN": "Vietnamese",
    "fa-IR": "Persian",
    "uk-UA": "Ukrainian",
    "cs-CZ": "Czech",
    "sv-SE": "Swedish"
}

base_file = "/home/steven/source/melodee/src/Melodee.Blazor/Resources/SharedResources.resx"
output_dir = "/home/steven/source/melodee/src/Melodee.Blazor/Resources"

common_translations = {
    "nl-NL": {"Dashboard": "Dashboard", "Library": "Bibliotheek", "Artists": "Artiesten", "Albums": "Albums", "Songs": "Nummers", "Playlists": "Afspeellijsten", "Search": "Zoeken", "Settings": "Instellingen", "Save": "Opslaan", "Cancel": "Annuleren", "Delete": "Verwijderen"},
    "pl-PL": {"Dashboard": "Pulpit", "Library": "Biblioteka", "Artists": "Artyści", "Albums": "Albumy", "Songs": "Utwory", "Playlists": "Playlisty", "Search": "Szukaj", "Settings": "Ustawienia", "Save": "Zapisz", "Cancel": "Anuluj", "Delete": "Usuń"},
    "tr-TR": {"Dashboard": "Panel", "Library": "Kitaplık", "Artists": "Sanatçılar", "Albums": "Albümler", "Songs": "Şarkılar", "Playlists": "Oynatma Listeleri", "Search": "Ara", "Settings": "Ayarlar", "Save": "Kaydet", "Cancel": "İptal", "Delete": "Sil"},
    "id-ID": {"Dashboard": "Dasbor", "Library": "Perpustakaan", "Artists": "Artis", "Albums": "Album", "Songs": "Lagu", "Playlists": "Daftar Putar", "Search": "Cari", "Settings": "Pengaturan", "Save": "Simpan", "Cancel": "Batal", "Delete": "Hapus"},
    "ko-KR": {"Dashboard": "대시보드", "Library": "라이브러리", "Artists": "아티스트", "Albums": "앨범", "Songs": "곡", "Playlists": "재생 목록", "Search": "검색", "Settings": "설정", "Save": "저장", "Cancel": "취소", "Delete": "삭제"},
    "vi-VN": {"Dashboard": "Bảng điều khiển", "Library": "Thư viện", "Artists": "Nghệ sĩ", "Albums": "Album", "Songs": "Bài hát", "Playlists": "Danh sách phát", "Search": "Tìm kiếm", "Settings": "Cài đặt", "Save": "Lưu", "Cancel": "Hủy", "Delete": "Xóa"},
    "fa-IR": {"Dashboard": "داشبورد", "Library": "کتابخانه", "Artists": "هنرمندان", "Albums": "آلبوم‌ها", "Songs": "آهنگ‌ها", "Playlists": "لیست‌های پخش", "Search": "جستجو", "Settings": "تنظیمات", "Save": "ذخیره", "Cancel": "لغو", "Delete": "حذف"},
    "uk-UA": {"Dashboard": "Панель керування", "Library": "Бібліотека", "Artists": "Виконавці", "Albums": "Альбоми", "Songs": "Пісні", "Playlists": "Плейлисти", "Search": "Пошук", "Settings": "Налаштування", "Save": "Зберегти", "Cancel": "Скасувати", "Delete": "Видалити"},
    "cs-CZ": {"Dashboard": "Nástěnka", "Library": "Knihovna", "Artists": "Umělci", "Albums": "Alba", "Songs": "Skladby", "Playlists": "Seznamy skladeb", "Search": "Hledat", "Settings": "Nastavení", "Save": "Uložit", "Cancel": "Zrušit", "Delete": "Smazat"},
    "sv-SE": {"Dashboard": "Instrumentpanel", "Library": "Bibliotek", "Artists": "Artister", "Albums": "Album", "Songs": "Låtar", "Playlists": "Spellistor", "Search": "Sök", "Settings": "Inställningar", "Save": "Spara", "Cancel": "Avbryt", "Delete": "Ta bort"}
}

def implement_languages():
    tree = ET.parse(base_file)
    root = tree.getroot()

    for code, lang_name in languages.items():
        output_file = os.path.join(output_dir, f"SharedResources.{code}.resx")
        new_root = ET.Element("root")
        
        # Copy headers
        for child in root:
            if child.tag != "data":
                new_root.append(ET.fromstring(ET.tostring(child)))
            else:
                # Process data elements
                name = child.attrib.get("name")
                value_elem = child.find("value")
                if value_elem is not None:
                    english_value = value_elem.text
                    new_data = ET.SubElement(new_root, "data", name=name)
                    new_data.set("{http://www.w3.org/XML/1998/namespace}space", "preserve")
                    new_value = ET.SubElement(new_data, "value")
                    
                    translated = False
                    if code in common_translations and english_value in common_translations[code]:
                        new_value.text = common_translations[code][english_value]
                        translated = True
                    
                    if not translated:
                        new_value.text = f"[NEEDS TRANSLATION] {english_value}"
                
                comment_elem = child.find("comment")
                if comment_elem is not None:
                    new_comment = ET.SubElement(new_data, "comment")
                    new_comment.text = comment_elem.text

        # Pretty print
        ET.indent(new_root, space="  ", level=0)

        # Write to file
        with open(output_file, "wb") as f:
            f.write(b"<?xml version='1.0' encoding='utf-8'?>\n")
            f.write(ET.tostring(new_root, encoding="utf-8"))
        print(f"Created {output_file}")

if __name__ == "__main__":
    implement_languages()
