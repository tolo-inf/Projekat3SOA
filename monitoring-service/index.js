const mqtt = require('mqtt')
const clientId = 'monitoring'
const username = 'monitoring'
const password = 'monitoring'
const edgexTopic = 'edgex/sensor_value'
let currentState = 'OFF'
const request = require('request');
const axios = require('axios')

const address = 'tcp://broker.hivemq.com:1883'

const mqttClient = mqtt.connect(address, {
    clientId,
    username,
    password
})

mqttClient.subscribe(edgexTopic, () => {
    console.log(`monitoring service subscribed to ${edgexTopic}`)
})

mqttClient.on('message', (topic, payload) => {
    if (topic !== edgexTopic) return;

    const data = JSON.parse(payload.toString())
    if (data.device !== 'SensorValueCluster2') return;
    const temp = data.readings[0].value
    console.log(`Temperature is ${temp}`)

    if (temp < 30 && currentState === 'OFF')
    {
        currentState = 'ON'
        console.log('CURRENT STATE CHANGED TO ON')
        sendAlert()
        return
    }
    
    if (temp > 40 && currentState === 'ON')
    {
        currentState = 'OFF'
        console.log('CURRENT STATE CHANGED TO OFF')
        sendAlert()
    }
})

async function sendAlert()
{
    const url = "http://command:48082/api/v1/device/4ce4f666-06d5-4caa-b61a-03c2622768c6/command/6ed15951-a9c8-45b3-9a26-f50f0cfecd42"
    const body = {
        color: currentState === 'OFF' ? "red" : "green"
    }

    try {
        const response = await axios.put(url, {
            color: currentState === 'OFF' ? "red" : "green"
        })
        console.log(response)
    }
    catch (ex) {
        console.log(ex)
    }
}