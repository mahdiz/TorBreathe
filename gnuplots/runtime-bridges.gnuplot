reset
set size 1.0,1.0
#set terminal postscript enhanced color font ',20'
set autoscale
set output "torbrix.eps"
set xlabel "Number of corrupt users" font ",20"
set xtics font ",16"
set ytics font ",16"
set key top left

plot [t=1:512] (10*t + 96)*(log10(1024)/log10(2))

#fit f1(x) 'atomic_COL_id.dat' using 1:2 via m,b
#plot "runtime-bridges.dat" using 1:2 title 'Running Time' with yerrorbars lt rgb "blue" ps 2, \
#	 "runtime-bridges.dat" using 1:3 title 'Bridges used' with linespoint lt rgb "green" ps 2, \
	#"runtime-bridges.dat" using 1:3 title 'Disjunction' with linespoint lt rgb "red" ps 2, \
	#"runtime-bridges.dat" using 1:5 title 'Disjunction-Multithreaded' with linespoint lt rgb "brown" ps 2
