import os
import chardet
import re

def read_comments_from_txt(txt_file):
    results = {}
    try:
        with open(txt_file, 'rb') as file:
            raw_data = file.read()
            result = chardet.detect(raw_data)
            encoding = result['encoding']
        
        with open(txt_file, 'r', encoding=encoding) as file:
            current_file = None
            for line in file:
                if line.startswith('File: '):
                    current_file = line[len('File: '):].strip()
                    results[current_file] = []
                elif line.startswith('Line '):
                    line_num, comment = line[len('Line '):].split(': ', 1)
                    results[current_file].append((int(line_num), comment.strip()))
    except Exception as e:
        print(f"Failed to read {txt_file}: {e}")
    return results

def remove_comments_from_line(line):
    # Remove single line comments
    line = re.sub(r'//.*', '', line)
    # Remove block comments
    line = re.sub(r'/\*.*?\*/', '', line)
    # Remove XML documentation comments
    line = re.sub(r'///.*', '', line)
    # Remove [Header("...")]
    line = re.sub(r'\[Header\(.*?\)\]', '', line)
    return line

def insert_comments_into_file(file_path, comments):
    try:
        with open(file_path, 'rb') as file:
            raw_data = file.read()
            result = chardet.detect(raw_data)
            encoding = result['encoding']
        
        with open(file_path, 'r', encoding=encoding) as file:
            lines = file.readlines()

        with open(file_path, 'w', encoding=encoding) as file:
            for line_num, comment in sorted(comments, reverse=True):
                # Remove original comment from the line, but keep the code
                lines[line_num - 1] = remove_comments_from_line(lines[line_num - 1]).rstrip() + "\n"
                # Insert new comment
                lines.insert(line_num, f"{comment}\n")
            file.writelines(lines)
    except Exception as e:
        print(f"Failed to insert comments into {file_path}: {e}")

def insert_comments(root_dir, comments):
    for relative_path, file_comments in comments.items():
        file_path = os.path.join(root_dir, relative_path)
        if os.path.exists(file_path):
            insert_comments_into_file(file_path, file_comments)
            print(f"Comments inserted into {file_path}")

def main():
    root_dir = os.path.dirname(os.path.abspath(__file__))
    txt_file = os.path.join(root_dir, 'comments.txt')
    
    if not os.path.exists(txt_file):
        print("Comments file not found.")
        input("Press any key to exit...")
        return
    
    comments = read_comments_from_txt(txt_file)
    
    if not comments:
        print("No comments to insert.")
    else:
        insert_comments(root_dir, comments)
        print("All comments inserted successfully.")
    
    input("Press any key to exit...")

if __name__ == "__main__":
    main()
