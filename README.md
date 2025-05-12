# UsufulPrograms
研究等で使えそうなコード一覧（生成AIを使って作成したものを含む）

## LocCounter
.csファイルのLOCをカウントする．そのまま数えた場合と空行，コメントアウトを除いた場合の2種類を出力する．LOCをカウントしたいファイルは./TargetProgramsに格納する．複数ファイルの総行数をカウントすることも可能．

## LocDuplicationCounter
.csファイルの重複LOCをカウントする．空行やコメントアウトは無視する．LOCをカウントしたいファイルは./TargetProgramsに格納する．3つ以上のファイルに対してもカウント可能．