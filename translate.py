import os
import openai

# 从环境变量中获取 OpenAI API 密钥，GitHub Actions 会将密钥作为 Secret 注入到环境中
openai.api_key = os.getenv('OPENAI_API_KEY')  # GitHub Action 自动注入密钥

# 翻译函数
def translate_text(text, target_lang):
    # 构建翻译提示
    prompt = f"Translate the following text to {target_lang}: {text}"
    
    # 调用 OpenAI API 进行翻译
    response = openai.Completion.create(
        model="gpt-4o-mini",  # 使用 GPT-4o-mini 模型
        prompt=prompt,
        max_tokens=1000,
        temperature=0.3
    )
    
    # 获取翻译结果
    translation = response.choices[0].text.strip()
    return translation

# 读取文件并逐行翻译
def translate_file(file_name, target_lang):
    translated_lines = []
    
    with open(file_name, "r", encoding="utf-8") as file:
        for line in file:
            # 对每一行文本进行翻译
            if line.strip():  # 跳过空行
                translated_line = translate_text(line.strip(), target_lang)
                translated_lines.append(translated_line)
            else:
                translated_lines.append("")  # 空行也保留
    
    # 将所有翻译后的行合并成文本并返回
    return "\n".join(translated_lines)

# 翻译CHANGELOG文件
changelog_en = translate_file(r"D:\Unity Projects\Undertale-Changer-Template\CHANGELOG_zh-CN.md", "English")  # 翻译为英语
changelog_zh_tw = translate_file(r"D:\Unity Projects\Undertale-Changer-Template\CHANGELOG_zh-CN.md", "Traditional Chinese")  # 翻译为繁体中文

# 翻译PLAN文件
plan_en = translate_file(r"D:\Unity Projects\Undertale-Changer-Template\PLAN_zh-CN.md", "English")  # 翻译为英语
plan_zh_tw = translate_file(r"D:\Unity Projects\Undertale-Changer-Template\PLAN_zh-CN.md", "Traditional Chinese")  # 翻译为繁体中文

# 将翻译后的文本写入文件
with open(r"D:\Unity Projects\Undertale-Changer-Template\CHANGELOG.md", "w", encoding="utf-8") as file:
    file.write(changelog_en)

with open(r"D:\Unity Projects\Undertale-Changer-Template\PLAN.md", "w", encoding="utf-8") as file:
    file.write(plan_en)

with open(r"D:\Unity Projects\Undertale-Changer-Template\CHANGELOG_zh-TW.md", "w", encoding="utf-8") as file:
    file.write(changelog_zh_tw)

with open(r"D:\Unity Projects\Undertale-Changer-Template\PLAN_zh-TW.md", "w", encoding="utf-8") as file:
    file.write(plan_zh_tw)

print("Files translated and saved.")
