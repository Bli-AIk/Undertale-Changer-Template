import os
from pygoogletranslation import Translator

# 初始化翻译器
translator = Translator()

# 翻译函数
def translate_text(text, target_lang):
    translation = translator.translate(text, dest=target_lang)
    print("Translation Result:", translation.text)
    return translation.text

# 读取文件并逐行翻译
def translate_file(file_name, target_lang):
    translated_lines = []
    
    with open(file_name, "r", encoding="utf-8") as file:
        for line in file:
            # 对每一行文本进行翻译py
            if line.strip():  # 跳过空行
                translated_line = translate_text(line.strip(), target_lang)
                translated_lines.append(translated_line)
            else:
                translated_lines.append("")  # 空行也保留
    
    # 将所有翻译后的行合并成文本并返回
    return "\n".join(translated_lines)

# 翻译CHANGELOG文件
changelog_en = translate_file(r"D:\Unity Projects\Undertale-Changer-Template\CHANGELOG_zh-CN.md", "en")  # 翻译为英语
changelog_zh_tw = translate_file(r"D:\Unity Projects\Undertale-Changer-Template\CHANGELOG_zh-CN.md", "zh-tw")  # 翻译为繁体中文

# 翻译PLAN文件
plan_en = translate_file(r"D:\Unity Projects\Undertale-Changer-Template\PLAN_zh-CN.md", "en")  # 翻译为英语
plan_zh_tw = translate_file(r"D:\Unity Projects\Undertale-Changer-Template\PLAN_zh-CN.md", "zh-tw")  # 翻译为繁体中文

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
