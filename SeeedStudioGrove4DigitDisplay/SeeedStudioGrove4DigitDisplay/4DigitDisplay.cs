using System.IO.Ports;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware.NetduinoPlus;

namespace SeeedStudio.Grove.4DigitDisplay
{
    class Display
    {
        // Definitions for TM1637
        private byte ADDR_AUTO = { 0x40 };
        private byte ADDR_FIXED = { 0x44 };

        private byte STARTADDR = { 0xc0 };

        // Definitions for the clock point of the digit tube
        private bool POINT_ON = true;
        private bool POINT_OFF = false;

        private int BRIGHT_DARKEST = 0;
        private int BRIGHT_TYPICAL = 2;
        private int BRIGHTEST = 7;

        //0~9,A,b,C,d,E,F   
        private byte[] TubeTab = {0x3f,0x06,0x5b,0x4f,
                           0x66,0x6d,0x7d,0x07,
                           0x7f,0x6f,0x77,0x7c,
                           0x39,0x5e,0x79,0x71};

        public byte Cmd_SetData;
        public byte Cmd_SetAddr;
        public byte Cmd_DispCtrl;
        public bool _PointFlag;

        private static OutputPort Clkpin = new OutputPort(Pins.GPIO_PIN_D5, false);
        private static TristatePort Datapin = new TristatePort(Pins.GPIO_PIN_D6, false, false, Port.ResistorMode.Disabled);

        public void writeByte(byte wr_data)
        {
            int i, count1;

            for(i=0;i<8;i++)        //sent 8bit data
            {
                Clkpin.Write(false);     
                if((wr_data & 0x01) > 0)
                    Datapin.Write(true);
                else 
                    Datapin.Write(false);
                wr_data >>= 1;      
                Clkpin.Write(true);
            }
            Clkpin.Write(false);
            Datapin.Write(true);
            Clkpin.Write(true);

            while(digitalRead(Datapin))    
            { 
                count1 +=1;
                if(count1 == 200)//
                {
                    pinMode(Datapin,OUTPUT);
                    digitalWrite(Datapin,LOW);
                    count1 = 0;
                }
                pinMode(Datapin,INPUT);
            }
            pinMode(Datapin,OUTPUT);
        }

        public void display(byte BitAddr, byte DispData)
        {
              byte SegData;
              SegData = coding(DispData);
              start();          //start signal sent to TM1637 from MCU
              writeByte(ADDR_FIXED);
              stop();
              start();
              writeByte(BitAddr|(byte)0xc0);
              writeByte(SegData);
              stop();
              start();
              writeByte(Cmd_DispCtrl);
              stop();
        }

        public void ClearDisplay()
        {
            display(0x00,0x7f);
            display(0x01,0x7f);
            display(0x02,0x7f);
            display(0x03,0x7f);  
        }

        public void start()
        {
            Clkpin.Write(true);
            Datapin.Write(true);
            Datapin.Write(false);
            Clkpin.Write(false);
        } 

        //End of transmission
        public void stop()
        {
            Clkpin.Write(false);
            Datapin.Write(false);
            Clkpin.Write(true);
            Datapin.Write(true);
        }

        //To take effect the next time it displays.
        public void set(byte brightness, byte SetData, byte SetAddr)
        {
          Cmd_SetData = SetData;
          Cmd_SetAddr = SetAddr;
          Cmd_DispCtrl = (byte)(0x88 + brightness);//Set the brightness and it takes effect the next time it displays.
        }

        //Whether to light the clock point ":".
        //To take effect the next time it displays.
        public void Tpoint(bool PointFlag)
        {
          _PointFlag = PointFlag;
        }

/*        public void coding(byte[] DispData[])
        {
            byte PointData;

            if(_PointFlag == POINT_ON)
                PointData = 0x80;
            else 
                PointData = 0;

            for(uint8_t i = 0;i < 4;i ++)
            {
                if(DispData[i] == 0x7f)DispData[i] = 0x00;
                else DispData[i] = TubeTab[DispData[i]] + PointData;
            }
        }
*/
        public byte coding(byte DispData)
        {
          byte PointData;
          if(_PointFlag == POINT_ON)
              PointData = 0x80;
          else 
              PointData = 0; 
          if(DispData == 0x7f) 
              DispData = (byte)(0x00 + PointData);//The bit digital tube off
          else 
              DispData = TubeTab[DispData] + PointData;
          return DispData;
        }
    }
}