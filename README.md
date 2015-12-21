# BPM分析を書いてみる

## 楽曲データ
サンプリング周波数$F[Hz]$でサンプリングされた楽曲データを$D$とし、
$n$番目の値を$D_n$、サンプル数を$N$とする

## 音量検出
分析に使用するフレームサイズを$W$とすると$n$番目のフレームの音量$V(n)$は
\[
V_n=\sum_{ｍ=nW}^{(n+1)W} D_ｍ^2
\]
$V$のデータ数$N$は$W$の整数倍であることを前提とする。この時、n番目のフレームの時刻は
\[
t_n=n\frac{W}{F}
\]
で与えられる。

## 自己相関をとる
音量のデータ$V$をｎフレームずつずらして内積を取る。

\[\begin{eqnarray}
C_0&=&V_0\cdot V_0+V_1\cdot V_1+\cdots+V_N\cdot V_N\\
C_1&=&V_0\cdot V_1+V_1\cdot V_2+\cdots+V_{N-1}\cdot V_N\\
&\cdots&\\
C_n&=&V_0\cdot V_n+V_1\cdot V_{n+1}+\cdots+V_{N-n}\cdot V_N\\
書き直すと\\
C_n&=&\sum_{m=0}^{N-n} (V_m\cdot V_{m+n})
\end{eqnarray}\]

## 一次近似による補正
自己相関をグラフ化するとずらすフレーム数が増えるほど相関が小さくなる傾向にあるので、一次近似直線で補正してからBPM検出を行う。

###　一次近似直線を求める
近似直線の式を
\[
y_n = at_n+b
\]
としてパラメータ$a$,$b$を求める。
$C_n$のデータ数を$N_C$とすると
\[
\begin{eqnarray}
P &=& \sum_{n=0}^{N_c} t_n \\
Q &=& \sum_{n=0}^{N_c} C_n \\
R &=& \sum_{n=0}^{N_c}(t_nC_n) \\
S &=& \sum_{n=0}^{N_c} t_n^2
\end{eqnarray}
\]
とすると、
\[\begin{eqnarray}
a &=& \frac{N_cR-PQ}{N_cS-P^2} \\
b &=& \frac{SQ-RP}{N_cS-P^2} \\
\end{eqnarray}\]
要は
\[\begin{eqnarray}
a &=& \frac{N_c \sum(t_nC_n)-\sum t_n \sum C_n}{N_c \sum t_n^2-(\sum t_n)^2} \\
b &=& \frac{N_c \sum t_n^2 \sum C_n-\sum t_nC_n \sum t_n}{N_c \sum t_n^2-(\sum t_n)^2}
\end{eqnarray}\]
ああ、ややこしい。

### 補正
なんだかんだで近似式が求められるので、これで自己相関を補正する。補正式は
\[
G_n=C_n-at_n-b
\]

## BPMを計算する
### 波動関数
BPM抽出のための波動関数を定義する。まず、
\[
f_{bpm}=\frac{bpm}{60}\\
\]
とすると、波動関数は
\[
B_{bpm}(t)=0.4\cos(2\pi f_{bpm}t) + 0.6\cos(2\cdot2\pi f_{bpm} t )
\]
で与えられるものとする。

###マッチ度の計算
BPMごとのマッチ度は
\[\begin{eqnarray}
M_{bpm}&=&\sum_{n=0}^{N_c} \left( G_n - B\left(t_n\right)\right) \\
&=&\sum_{n=0}^{N_c}\left(G_n -0.4\cos\left(2\pi f_{bpm}t_n\right) - 0.6\cos\left(2\cdot2\pi f_{bpm} t_n \right)\right)
\end{eqnarray}\]
で求められる

###ピーク抽出

$M_{bpm-1}<M_{bpm}<M_{bpm+1}$
となるような$M_{bpm}$をピークとみなし、値が大きい順にソートした配列$P_n$を抽出する

###BPMを特定する
$P_n$で、優先するBPMの範囲$BPM_{min}$と$BPM_{max}$
および閾値$h(0<h<1)$が与えられた場合、
\[
BPM_{min}\leq P_n \leq BPM_{max}\\
および\\
P_n>P_0h
\]
を満たす$P_n$をBPM値として特定する。  上記を満たすものがない場合は$P_0$をBPM値とする

##各種定数
###対象とするBPMの範囲
実装上は  
$50 \leq bpm\leq250$
までを計算し、優先範囲として  
$BPM_{min}=80  \\
BPM_{max}=159$  
としている。

###フレームサイズ
フレームサイズ$W$はCDのサンプリング周波数が44.1[kHz]であることから、  
$W=4410$  
としている。時間で示すと1フレームの長さは0.1秒である。

###自己相関のサイズ
自己相関$C_n$は  
$0 \leq n \leq 50$  
の範囲で計算を行っている。  
時間にして5秒分に相当する。
