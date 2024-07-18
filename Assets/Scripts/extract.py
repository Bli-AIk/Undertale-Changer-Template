import os
import re
import chardet

def extract_comments_from_file(file_path):
    comments = []
    try:
        with open(file_path, 'rb') as file:
            raw_data = file.read()
            result = chardet.detect(raw_data)
            encoding = result['encoding']
            print(f"Reading {file_path} with encoding {encoding}")
            text = raw_data.decode(encoding)
            lines = text.splitlines()
            for line_num, line in enumerate(lines, start=1):
                matches = re.findall(r'//.*|/\*[\s\S]*?\*/|///.*|\[Header\(.*?\)\]', line)
                for match in matches:
                    comments.append((line_num, match.strip()))
    except Exception as e:
        print(f"Failed to read {file_path}: {e}")
    return comments

def scan_directory(root_dir):
    results = []
    try:
        for subdir, _, files in os.walk(root_dir):
            for file in files:
                if file.endswith('.cs'):
                    file_path = os.path.join(subdir, file)
                    comments = extract_comments_from_file(file_path)
                    if comments:
                        relative_path = os.path.relpath(file_path, root_dir)
                        results.append((relative_path, comments))
    except Exception as e:
        print(f"Failed to scan directory {root_dir}: {e}")
    return results

def save_results_to_txt(results, output_file):
    try:
        with open(output_file, 'w', encoding='utf-8') as file:
            for file_path, comments in results:
                file.write(f'File: {file_path}\n')
                for line_num, comment in comments:
                    file.write(f'Line {line_num}: {comment}\n')
                file.write('\n')
    except Exception as e:
        print(f"Failed to save results to {output_file}: {e}")

def main():
    try:
        root_dir = os.path.dirname(os.path.abspath(__file__))
        print(f"Scanning directory: {root_dir}")
        results = scan_directory(root_dir)
        
        if not results:
            print("No comments found.")
        else:
            output_file = os.path.join(root_dir, 'comments.txt')
            save_results_to_txt(results, output_file)
            print(f"Comments extracted and saved to {output_file}")
            for file_path, comments in results:
                print(f"File: {file_path}")
                for line_num, comment in comments:
                    print(f"  Line {line_num}: {comment}")
        
        input("Press any key to exit...")
    except Exception as e:
        print(f"An error occurred: {e}")
        input("Press any key to exit...")

if __name__ == "__main__":
    main()
