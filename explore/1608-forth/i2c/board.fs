\ board definitions
\ needs always.fs

eraseflash
compiletoflash
( board start: ) here dup hex.

include ../mlib/cond.fs
include ../mlib/hexdump.fs
include ../flib/stm32l0/io.fs
include ../flib/stm32l0/hal.fs
include ../flib/stm32l0/adc.fs
include ../flib/stm32l0/timer.fs
include ../flib/stm32l0/pwm.fs
include ../flib/stm32l0/spi.fs
include ../flib/stm32l0/i2c.fs

$77 constant BME.ADDR  \ due to solder jumper

PA15 constant LED

: led-on LED ioc! ;
: led-off LED ios! ;

: init ( -- )  \ board initialisation
  $00 hex.empty !  \ empty flash shows up as $00 iso $FF on these chips
  OMODE-PP LED io-mode!
\ 16MHz ( set by Mecrisp on startup to get an accurate USART baud rate )
  2 RCC-CCIPR !  \ set USART1 clock to HSI16, independent of sysclk
  flash-kb . ." KB <i2c> " hwid hex. ." ok." cr
  1000 systick-hz
;

: rx-connected? ( -- f )  \ true if RX is connected (and idle)
  IMODE-LOW PA10 io-mode!  PA10 io@ 0<>  OMODE-AF-PP PA10 io-mode!
  dup if 1 ms serial-key? if serial-key drop then then \ flush any input noise
;

: fake-key? ( -- f )  \ check for RX pin being pulled high
  rx-connected? if reset then false ;

\ unattended quits to the interpreter if the RX pin is connected, not floating
\ else it replaces the key? hook with a test to keep checking for RX reconnect
\ if so, it will reset to end up in the interpreter on the next startup
\ for use with a turnkey app in flash, i.e. ": init init unattended ... ;"

: unattended
  rx-connected? if quit then \ return to command prompt
  ['] fake-key? hook-key? ! ;

( board end, size: ) here dup hex. swap - .
cornerstone <<<board>>>
