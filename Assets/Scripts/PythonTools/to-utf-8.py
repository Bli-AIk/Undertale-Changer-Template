import os
import chardet

def convert_file_to_utf8(file_path):
    try:
        with open(file_path, 'rb') as file:
            raw_data = file.read()
            result = chardet.detect(raw_data)
            original_encoding = result['encoding']
        
        with open(file_path, 'r', encoding=original_encoding) as file:
            text = file.read()
        
        with open(file_path, 'w', encoding='utf-8') as file:
            file.write(text)
        
        print(f"Converted {file_path} from {original_encoding} to utf-8")
    except Exception as e:
        print(f"Failed to convert {file_path}: {e}")

def scan_and_convert_directory(root_dir):
    try:
        for subdir, _, files in os.walk(root_dir):
            for file in files:
                if file.endswith('.cs'):
                    file_path = os.path.join(subdir, file)
                    convert_file_to_utf8(file_path)
    except Exception as e:
        print(f"Failed to scan directory {root_dir}: {e}")

def main():
    root_dir = os.path.dirname(os.path.abspath(__file__))
    print(f"Scanning and converting .cs files in directory: {root_dir}")
    scan_and_convert_directory(root_dir)
    print("Conversion completed.")
    input("Press any key to exit...")

if __name__ == "__main__":
    main()
