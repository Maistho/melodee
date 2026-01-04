#!/usr/bin/env python3
"""
Normalize translation files by replacing language-specific placeholders
with uniform [NEEDS TRANSLATION] prefix followed by English text.

This makes it easier for contributors to:
1. See what needs translation
2. Understand the meaning from English text
3. Search for untranslated strings with a single pattern
"""

import xml.etree.ElementTree as ET
import os
import sys

# Language-specific placeholder patterns to replace
PLACEHOLDERS = {
    'es-ES': 'Falta traducción',
    'de-DE': 'Übersetzung fehlt',
    'it-IT': 'Traduzione mancante',
    'pt-BR': 'Tradução ausente',
    'ja-JP': '未翻訳',
    'ru-RU': 'Перевод отсутствует',
    'zh-CN': '缺少翻译',
    'ar-SA': 'ترجمة مفقودة',
    'fr-FR': 'Traduction manquante',  # In case any exist
}

def load_resource_file(filepath):
    """Load a .resx file and return a dict of key -> value"""
    tree = ET.parse(filepath)
    root = tree.getroot()
    resources = {}
    for data in root.findall('data'):
        name = data.get('name')
        value_elem = data.find('value')
        if name and value_elem is not None and value_elem.text:
            resources[name] = value_elem.text
    return resources, tree, root

def normalize_translation_file(base_file, target_file, lang_code):
    """Replace placeholder values with [NEEDS TRANSLATION] English text"""
    
    placeholder = PLACEHOLDERS.get(lang_code)
    if not placeholder:
        print(f"  No placeholder pattern defined for {lang_code}")
        return 0
    
    # Load English base file
    base_resources, _, _ = load_resource_file(base_file)
    
    # Load target translation file
    target_resources, tree, root = load_resource_file(target_file)
    
    # Track changes
    changes = 0
    
    for data in root.findall('data'):
        name = data.get('name')
        value_elem = data.find('value')
        
        if name and value_elem is not None and value_elem.text:
            current_value = value_elem.text.strip()
            
            # Check if this is a placeholder value
            if current_value == placeholder or current_value.startswith(placeholder):
                # Get English text as fallback
                english_text = base_resources.get(name, name)
                
                # Set new value with NEEDS TRANSLATION prefix
                new_value = f"[NEEDS TRANSLATION] {english_text}"
                value_elem.text = new_value
                changes += 1
    
    if changes > 0:
        # Write back the file
        tree.write(target_file, encoding='utf-8', xml_declaration=True)
        
        # Fix the XML header and formatting (resx files need specific format)
        with open(target_file, 'r', encoding='utf-8') as f:
            content = f.read()
        
        # Ensure proper XML declaration
        if not content.startswith('<?xml'):
            content = '<?xml version="1.0" encoding="utf-8"?>\n' + content
        
        with open(target_file, 'w', encoding='utf-8') as f:
            f.write(content)
    
    return changes

def main():
    resources_dir = 'src/Melodee.Blazor/Resources'
    base_file = os.path.join(resources_dir, 'SharedResources.resx')
    
    if not os.path.exists(base_file):
        print(f"Error: Base file not found: {base_file}")
        sys.exit(1)
    
    print("Normalizing translation files...")
    print()
    
    total_changes = 0
    
    for lang_code in PLACEHOLDERS.keys():
        target_file = os.path.join(resources_dir, f'SharedResources.{lang_code}.resx')
        
        if not os.path.exists(target_file):
            print(f"  {lang_code}: File not found, skipping")
            continue
        
        changes = normalize_translation_file(base_file, target_file, lang_code)
        total_changes += changes
        
        if changes > 0:
            print(f"  {lang_code}: Updated {changes} entries")
        else:
            print(f"  {lang_code}: No changes needed")
    
    print()
    print(f"Total entries normalized: {total_changes}")
    
    if total_changes > 0:
        print()
        print("Contributors can now find untranslated strings by searching for:")
        print("  [NEEDS TRANSLATION]")

if __name__ == '__main__':
    main()
