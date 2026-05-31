from pathlib import Path
from shutil import copy2

root = Path(r"E:\Project_Metroidvania\Metroidvania")
src = root / "葛广一" / "毕业设计论文最终版_补充关卡流程_平台路线修正版.docx"
dst = root / "output" / "thesis_final_platform_route_refined.docx"
copy2(src, dst)
print(dst)
