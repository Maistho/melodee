#!/usr/bin/env python3
"""
Script to calculate translation percentages and update CONTRIBUTING_TRANSLATIONS.md
"""

import os
import re
from pathlib import Path

def count_total_entries(file_path):
    """Count total entries in a resource file"""
    with open(file_path, 'r', encoding='utf-8') as f:
        content = f.read()
    # Count all <data name="..."> entries
    matches = re.findall(r'<data name="[^"]+"', content)
    return len(matches)

def count_untranslated_entries(file_path):
    """Count entries with NEEDS TRANSLATION"""
    with open(file_path, 'r', encoding='utf-8') as f:
        content = f.read()
    # Count all entries with [NEEDS TRANSLATION]
    matches = re.findall(r'\[NEEDS TRANSLATION\]', content)
    return len(matches)

def calculate_percentage(total, untranslated):
    """Calculate translation percentage"""
    if total == 0:
        return 100  # If no entries, consider 100% translated
    translated = total - untranslated
    return round((translated / total) * 100)

def main():
    # Base directory
    base_dir = Path("/home/steven/source/melodee")
    resources_dir = base_dir / "src" / "Melodee.Blazor" / "Resources"
    
    # Get the English file as reference
    english_file = resources_dir / "SharedResources.resx"
    total_entries = count_total_entries(english_file)
    
    print(f"Total entries in English file: {total_entries}")
    
    # Get all language files
    lang_files = list(resources_dir.glob("SharedResources.*.resx"))
    lang_files = [f for f in lang_files if f.name != "SharedResources.resx"]  # Exclude English
    
    # Calculate percentages for each language
    results = []
    for lang_file in lang_files:
        lang_code = lang_file.name.replace("SharedResources.", "").replace(".resx", "")
        
        # Count total entries in this language file
        total_lang_entries = count_total_entries(lang_file)
        
        # Count untranslated entries
        untranslated_count = count_untranslated_entries(lang_file)
        
        # Calculate percentage
        percentage = calculate_percentage(total_lang_entries, untranslated_count)
        
        results.append({
            'lang_code': lang_code,
            'file_name': lang_file.name,
            'percentage': percentage,
            'untranslated_count': untranslated_count
        })
        
        print(f"{lang_code}: {percentage}% translated ({untranslated_count} untranslated)")
    
    # Now update the CONTRIBUTING_TRANSLATIONS.md file
    md_file = base_dir / "CONTRIBUTING_TRANSLATIONS.md"
    
    with open(md_file, 'r', encoding='utf-8') as f:
        content = f.read()
    
    # Define language code to display name mapping
    lang_names = {
        'ar-SA': 'Arabic (Saudi Arabia)',
        'cs-CZ': 'Czech (Czechia)', 
        'de-DE': 'German',
        'es-ES': 'Spanish',
        'fa-IR': 'Persian (Iran)',
        'fr-FR': 'French',
        'id-ID': 'Indonesian (Indonesia)',
        'it-IT': 'Italian',
        'ja-JP': 'Japanese',
        'ko-KR': 'Korean (Korea)',
        'nl-NL': 'Dutch (Netherlands)',
        'pl-PL': 'Polish (Poland)',
        'pt-BR': 'Portuguese (Brazil)',
        'ru-RU': 'Russian',
        'sv-SE': 'Swedish (Sweden)',
        'tr-TR': 'Turkish (Turkey)',
        'uk-UA': 'Ukrainian (Ukraine)',
        'vi-VN': 'Vietnamese (Vietnam)',
        'zh-CN': 'Chinese (Simplified)'
    }
    
    # Define RTL languages
    rtl_languages = {'ar-SA', 'fa-IR'}
    
    # Update the table in the markdown file
    table_start_marker = "| Language | Code | File | Status | RTL Support |"
    table_end_marker = "## Translation Status"
    
    # Find the table section
    start_pos = content.find(table_start_marker)
    end_pos = content.find(table_end_marker, start_pos)
    
    if start_pos != -1 and end_pos != -1:
        # Extract the table header
        table_header = content[start_pos:end_pos]
        
        # Build the new table
        new_table = "| Language | Code | File | Status | RTL Support |\n"
        new_table += "|----------|------|------|--------|-------------|\n"
        
        # Add English row (always 100%)
        new_table += "| English (US) | en-US | `SharedResources.resx` | ✅ 100% | No |\n"
        
        # Sort results by language code for consistency
        results.sort(key=lambda x: x['lang_code'])
        
        for result in results:
            lang_code = result['lang_code']
            percentage = result['percentage']
            file_name = result['file_name']
            
            # Get display name
            display_name = lang_names.get(lang_code, lang_code)
            
            # Determine status icon
            if percentage == 100:
                status = f"✅ {percentage}%"
            else:
                status = f"🔄 {percentage}%"
                
            # Determine RTL support
            rtl_support = "Yes" if lang_code in rtl_languages else "No"
            
            new_table += f"| {display_name} | {lang_code} | `{file_name}` | {status} | {rtl_support} |\n"
        
        # Replace the old table with the new one
        new_content = content[:start_pos] + new_table + content[end_pos:]
        
        # Write the updated content back to the file
        with open(md_file, 'w', encoding='utf-8') as f:
            f.write(new_content)
        
        print("\nCONTRIBUTING_TRANSLATIONS.md has been updated with accurate percentages!")
    else:
        print("Could not find the table in CONTRIBUTING_TRANSLATIONS.md")

if __name__ == "__main__":
    main()