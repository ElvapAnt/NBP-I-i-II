import { Button } from '@mui/material'
import './ChatCard.css'
import { useNavigate } from 'react-router-dom'

export default function ChatCard({ username,chatUid })
{
    const navigate = useNavigate()
    return <div className="chatcard">
        <Button variant='text' id='Button'
            onClick={() => navigate(`/${chatUid}`)}>
            {username != null ? username : 'USERNAME NOT FOUND'}</Button>
    </div>
}