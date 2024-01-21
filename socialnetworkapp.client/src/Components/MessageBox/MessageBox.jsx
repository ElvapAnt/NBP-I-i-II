import SendSharpIcon from '@mui/icons-material/SendSharp';
import { useState } from 'react';
import './MessageBox.css'
import { Button} from '@mui/material';


export default function MessageBox({chatUid,sendTo,onSendMessage})
{
    const [inputState, setInputState] = useState('')
    
    const onInputChanged = (e) => {
        setInputState(e.target.value);
    };

    const onSendClicked = () => {
        if (inputState.trim() !== "") {
            onSendMessage(inputState,chatUid);
            console.log(inputState);
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