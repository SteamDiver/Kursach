#include<Wire.h>
#define SamplesCount 512
#define SampleRate 128

const int MPU_addr = 0x68; // I2C address of the MPU-6050
int16_t AcX, AcY, AcZ;
int16_t arr[SamplesCount];
void setup() {
  Wire.begin();
  Wire.setClock(400000L);
  Wire.beginTransmission(MPU_addr);
  Wire.write(0x6B);  // PWR_MGMT_1 register
  Wire.write(0);     // set to zero (wakes up the MPU-6050)
  Wire.endTransmission(true);
  Serial.begin(115200);
}
void loop() {
  double loopTime = (double)1 / (double)SampleRate * 1000;
  double t = (double)millis();
  for(int i=0; i<SamplesCount; i++){
    double t1 = (double)millis();
    Wire.beginTransmission(MPU_addr);
    Wire.write(0x3B);  // starting with register 0x3B (ACCEL_XOUT_H)
    Wire.endTransmission(false);
    Wire.requestFrom(MPU_addr, 14, true); // request a total of 14 registers
    AcX = Wire.read() << 8 | Wire.read(); // 0x3B (ACCEL_XOUT_H) & 0x3C (ACCEL_XOUT_L)
    AcY = Wire.read() << 8 | Wire.read(); // 0x3D (ACCEL_YOUT_H) & 0x3E (ACCEL_YOUT_L)
    AcZ = Wire.read() << 8 | Wire.read(); // 0x3F (ACCEL_ZOUT_H) & 0x40 (ACCEL_ZOUT_L)
    arr[i] = AcX + AcY + AcZ;

    double del = loopTime - (double)millis() + t1;
   
    delay(del);
  }
  double sampleRate = ((double)SamplesCount / (double)(millis() - t)) * 1000;
  Serial.println("block");
  Serial.println(SamplesCount);
  Serial.println(sampleRate);
  for(int i = 0; i < SamplesCount; i++){
    Serial.print(arr[i]);
    Serial.print(";");
  }
  Serial.println("\r\nend");
  
  memset(arr, 0, sizeof(arr));
  //Serial.print(","); Serial.print(AcY);
  //Serial.print(","); Serial.println(AcZ);
}
