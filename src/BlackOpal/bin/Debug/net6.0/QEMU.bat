@ECHO OFF
echo Starting QEMU...
"C:\Program Files\qemu\qemu-system-x86_64.exe" -cdrom "BlackOpal.iso" -m 256 -vga std -net nic,model=e1000
rem -vga vmware -device vmware-svga,vgamem_mb=32