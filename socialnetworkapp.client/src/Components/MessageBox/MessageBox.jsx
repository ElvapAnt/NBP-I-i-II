import SendSharpIcon from '@mui/icons-material/SendSharp';
import { useState } from 'react';
import './MessageBox.css'
import { CURRENT_USER,  chatController } from '../../Constants'
import { useNavigate } from 'react-router-dom'
import { Button} from '@mui/material';


export default function MessageBox({chatUid,sendTo,onSendMessage,onChooseEncryption})
{
    const [inputState, setInputState] = useState('')
    const navigate = useNavigate()
    const [openDialog, setOpenDialog] = useState(false)
    
    const onInputChanged = (e) => {
        setInputState(e.target.value);
    };

    const onSendClicked = () => {
        if (inputState.trim() !== "") {
            onSendMessage(inputState, sendTo);
            console.log(sendTo);
            setInputState(""); // Clear the input after sending
        }
    };

    return <div className="messagebox">
        <input className='messagebox-input' type="text" value={inputState} onChange={onInputChanged}></input>
        <Button className='messagebox-button' onClick={onSendClicked}
        ><SendSharpIcon />
    </Button>
    </div>
}