from pathlib import Path
import re

from docx import Document
from docx.enum.text import WD_ALIGN_PARAGRAPH, WD_LINE_SPACING
from docx.oxml.ns import qn
from docx.shared import Pt


SOURCE = Path(r"E:\Project_Metroidvania\Metroidvania\output\format_fix_working.docx")
OUT = Path(r"E:\Project_Metroidvania\Metroidvania\output\format_fix_working_fixed.docx")


CJK_RE = re.compile(r"[\u3400-\u9fff]")


def set_run_fonts(run, east_asia=None, ascii_font=None, size_pt=None):
    if ascii_font:
        run.font.name = ascii_font

    if size_pt is not None:
        run.font.size = Pt(size_pt)

    rpr = run._element.get_or_add_rPr()
    rfonts = rpr.rFonts
    if rfonts is None:
        rfonts = rpr._add_rFonts()

    if east_asia:
        rfonts.set(qn("w:eastAsia"), east_asia)

    if ascii_font:
        rfonts.set(qn("w:ascii"), ascii_font)
        rfonts.set(qn("w:hAnsi"), ascii_font)


def set_style_alignment(doc, style_name, alignment):
    style = doc.styles[style_name]
    style.paragraph_format.alignment = alignment


def set_keyword_paragraph(paragraph):
    fmt = paragraph.paragraph_format
    fmt.first_line_indent = Pt(0)
    fmt.line_spacing_rule = WD_LINE_SPACING.EXACTLY
    fmt.line_spacing = Pt(20)

    for run in paragraph.runs:
        set_run_fonts(run, east_asia="黑体", ascii_font="Times New Roman", size_pt=10.5)


def fix_body_chinese_font(paragraph):
    if not CJK_RE.search(paragraph.text):
        return

    if paragraph.text.strip().startswith("关键词："):
        return

    for run in paragraph.runs:
        if CJK_RE.search(run.text):
            set_run_fonts(run, east_asia="宋体")


doc = Document(SOURCE)

# Keep style defaults and direct paragraph formatting aligned with the report comments.
set_style_alignment(doc, "TOC1", WD_ALIGN_PARAGRAPH.DISTRIBUTE)
set_style_alignment(doc, "TOC2", WD_ALIGN_PARAGRAPH.DISTRIBUTE)
set_style_alignment(doc, "TOC3", WD_ALIGN_PARAGRAPH.DISTRIBUTE)

chapter_style = doc.styles["章的标题"]
chapter_style.paragraph_format.space_after = Pt(1)

section_style = doc.styles["节标题"]
section_style.paragraph_format.space_before = Pt(0.5)
section_style.paragraph_format.alignment = WD_ALIGN_PARAGRAPH.JUSTIFY

subsection_style = doc.styles["段标题"]
subsection_style.paragraph_format.space_before = Pt(0.5)
subsection_style.paragraph_format.alignment = WD_ALIGN_PARAGRAPH.JUSTIFY

reference_style = doc.styles["参考文献"]
reference_style.paragraph_format.first_line_indent = Pt(0)
reference_style.paragraph_format.left_indent = Pt(0)
reference_style.paragraph_format.alignment = WD_ALIGN_PARAGRAPH.LEFT

summary_style = doc.styles["中文摘要"]
summary_style.paragraph_format.space_before = Pt(2)
summary_style.paragraph_format.line_spacing_rule = WD_LINE_SPACING.MULTIPLE
summary_style.paragraph_format.line_spacing = 1.5

counts = {
    "summary_heading": 0,
    "zh_keywords": 0,
    "en_keywords": 0,
    "chapter_heading": 0,
    "section_heading": 0,
    "subsection_heading": 0,
    "references": 0,
    "body_font": 0,
}

for paragraph in doc.paragraphs:
    text = paragraph.text.strip()
    style_name = paragraph.style.name
    fmt = paragraph.paragraph_format

    if style_name == "中文摘要" and text.replace(" ", "") == "摘要":
        fmt.space_before = Pt(2)
        fmt.line_spacing_rule = WD_LINE_SPACING.MULTIPLE
        fmt.line_spacing = 1.5
        counts["summary_heading"] += 1

    if text.startswith("关键词："):
        set_keyword_paragraph(paragraph)
        counts["zh_keywords"] += 1

    if text.startswith("Key words:"):
        fmt.line_spacing_rule = WD_LINE_SPACING.SINGLE
        fmt.line_spacing = 1
        counts["en_keywords"] += 1

    if style_name == "章的标题":
        fmt.space_after = Pt(1)
        counts["chapter_heading"] += 1

    if style_name == "节标题":
        fmt.space_before = Pt(0.5)
        paragraph.alignment = WD_ALIGN_PARAGRAPH.JUSTIFY
        counts["section_heading"] += 1

    if style_name == "段标题":
        fmt.space_before = Pt(0.5)
        paragraph.alignment = WD_ALIGN_PARAGRAPH.JUSTIFY
        counts["subsection_heading"] += 1

    if style_name == "参考文献":
        fmt.first_line_indent = Pt(0)
        fmt.left_indent = Pt(0)
        paragraph.alignment = WD_ALIGN_PARAGRAPH.LEFT
        counts["references"] += 1

    if style_name == "论文正文" and CJK_RE.search(paragraph.text):
        before = sum(
            1 for run in paragraph.runs if CJK_RE.search(run.text)
        )
        fix_body_chinese_font(paragraph)
        counts["body_font"] += before

doc.save(OUT)
print(f"wrote={OUT}")
for key, value in counts.items():
    print(f"{key}={value}")
