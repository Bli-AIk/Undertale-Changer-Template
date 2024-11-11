import http.client
import json
import os

# 设置 OpenAI API 密钥
api_key = os.getenv("OPENAI_API_KEY")
api_host = "api.baicaigpt.cn"
api_endpoint = "/v1/chat/completions"

# 翻译函数
def translate_text(text, target_lang):

    conn = http.client.HTTPSConnection(api_host)
    payload = json.dumps({
        "model": "gpt-4o-mini",
        "messages": [
            {
                "role": "system",
                "content": "You are a professional, authentic machine translation engine."
            },
            {
                "role": "user",
                content = f"Translate the following source text to {target_lang}. Output the translation directly without any additional text.\nSource Text: {text}\nTranslated Text:"
            }
        ]
    })
    headers = {
        'Authorization': f'Bearer {api_key}',
        'Content-Type': 'application/json'
    }
    
    try:
        conn.request("POST", api_endpoint, payload, headers)
        res = conn.getresponse()
        data = res.read().decode("utf-8")
        response = json.loads(data)
        # 获取翻译结果
        translation = response['choices'][0]['message']['content']
        return translation.strip()  # 移除首尾多余的空格
    except Exception as e:
        print(f"Error during translation: {e}")
        return None

# 读取文件并逐行翻译
def translate_file(file_name, target_lang):
    translated_lines = []
    
    with open(file_name, "r", encoding="utf-8") as file:
        for line in file:
            if line.strip():  # 跳过空行
                translated_line = translate_text(line.strip(), target_lang)
                if translated_line:
                    translated_lines.append(translated_line)
                else:
                    translated_lines.append(line.strip())  # 如果翻译失败，保留原文
            else:
                translated_lines.append("")  # 空行也保留
    
    return "\n".join(translated_lines)

# 示例调用
if __name__ == "__main__":
    changelog_en = translate_file("CHANGELOG_zh-CN.md", "English")
    changelog_zh_tw = translate_file("CHANGELOG_zh-CN.md", "Traditional Chinese")
    
    plan_en = translate_file("PLAN_zh-CN.md", "English")
    plan_zh_tw = translate_file("PLAN_zh-CN.md", "Traditional Chinese")
    
    # 将翻译后的文本写入文件
    with open("CHANGELOG.md", "w", encoding="utf-8") as file:
        file.write(changelog_en)

    with open("PLAN.md", "w", encoding="utf-8") as file:
        file.write(plan_en)

    with open("CHANGELOG_zh-TW.md", "w", encoding="utf-8") as file:
        file.write(changelog_zh_tw)

    with open("PLAN_zh-TW.md", "w", encoding="utf-8") as file:
        file.write(plan_zh_tw)

    print("Files translated and saved.")