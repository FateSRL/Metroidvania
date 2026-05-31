from pathlib import Path
import re

from docx import Document
from docx.oxml.ns import qn
from docx.shared import Pt


SOURCE = Path(r"E:\Project_Metroidvania\Metroidvania\output\format_fix_latest_working.docx")
OUT = Path(r"E:\Project_Metroidvania\Metroidvania\output\format_fix_latest_working_fixed.docx")


CJK_RE = re.compile(r"[\u3400-\u9fff]")


def set_run_east_asia_font(run, east_asia):
    rpr = run._element.get_or_add_rPr()
    rfonts = rpr.rFonts
    if rfonts is None:
        rfonts = rpr._add_rFonts()
    rfonts.set(qn("w:eastAsia"), east_asia)


doc = Document(SOURCE)

chapter_style = doc.styles["章的标题"]
chapter_style.paragraph_format.space_after = Pt(1)

section_style = doc.styles["节标题"]
section_style.paragraph_format.space_before = Pt(0.5)

subsection_style = doc.styles["段标题"]
subsection_style.paragraph_format.space_before = Pt(0.5)

counts = {
    "summary_heading": 0,
    "zh_keywords": 0,
    "chapter_heading": 0,
    "section_heading": 0,
    "subsection_heading": 0,
    "body_font_runs": 0,
}

for paragraph in doc.paragraphs:
    text = paragraph.text.strip()
    style_name = paragraph.style.name
    fmt = paragraph.paragraph_format

    if style_name == "中文摘要" and text.replace(" ", "") == "摘要":
        fmt.space_before = Pt(2)
        counts["summary_heading"] += 1

    if text.startswith("关键词："):
        fmt.first_line_indent = Pt(0)
        counts["zh_keywords"] += 1

    if style_name == "章的标题":
        fmt.space_after = Pt(1)
        counts["chapter_heading"] += 1

    if style_name == "节标题":
        fmt.space_before = Pt(0.5)
        counts["section_heading"] += 1

    if style_name == "段标题":
        fmt.space_before = Pt(0.5)
        counts["subsection_heading"] += 1

    if style_name == "论文正文" and CJK_RE.search(paragraph.text):
        for run in paragraph.runs:
            if CJK_RE.search(run.text):
                set_run_east_asia_font(run, "宋体")
                counts["body_font_runs"] += 1

doc.save(OUT)
print(f"wrote={OUT}")
for key, value in counts.items():
    print(f"{key}={value}")
