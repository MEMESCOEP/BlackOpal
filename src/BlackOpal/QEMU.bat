@ECHO OFF
echo Starting QEMU...
"C:\Program Files\qemu\qemu-system-x86_64.exe" -cdrom "bin/debug/net6.0/BlackOpal.iso" -m 128 -vga std -net nic,model=e1000