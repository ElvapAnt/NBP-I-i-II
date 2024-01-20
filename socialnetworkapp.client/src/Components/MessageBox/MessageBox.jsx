import SendSharpIcon from '@mui/icons-material/SendSharp';
import { useState } from 'react';
import './MessageBox.css'
import { CURRENT_USER,  chatController } from '../../Constants'
import { useNavigate } from 'react-router-dom'
import { Button} from '@mui/material';


export default function MessageBox({chatUid,sendTo})
{
    const [inputState, setInputState] = useState('')
    const navigate = useNavigate()

    
    function onChange(ev)
    {
        setInputState(ev.target.value)
    }

    async function sendMessage(chatUid, sendTo)
    {
        if (chatUid != undefined || sendTo != undefined)
        {
            const request = await fetch(`${chatController}/SendMessage`,
                {
                    method: "POST",
                    headers: {
                        "Content-type": "application/json"
                    },
                    body: JSON.stringify({
                        timestamp: Date.now(),
                        content: inputState,
                        sender: JSON.parse(localStorage.getItem(CURRENT_USER)).username,
                        recipient: sendTo,
                        chatUid: chatUid
                    })
                })
            if (request.ok) {
                if (chatUid == undefined) {
                    const chatId = await request.text()
                    navigate(`/${chatId}`)
                }
                setInputState('')
                setFileState(undefined)
            }
        
            
        }

    }

    
    const [openDialog, setOpenDialog] = useState(false)
    
    
    return <div className="messagebox">
        <input className='messagebox-input' type="text" value={inputState} onChange={onChange}></input>
        <Button className='messagebox-button' onClick={(ev) => sendMessage(chatUid, sendTo)} disabled={
            chatUid == undefined
            &&sendTo == undefined}><SendSharpIcon /></Button>
            </div>
}