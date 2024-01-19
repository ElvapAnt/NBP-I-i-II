import {useLoaderData,useNavigate } from "react-router-dom"
import SearchBar from "../../Components/Searchbar/Searchbar"
import './Chat.css'
import { Button } from "@mui/material"
import ChatCard from "../../Components/ChatCard/ChatCard"
import ChatBubble from "../../Components/ChatBubble/ChatBubble"
import MessageBox from "../../Components/MessageBox/MessageBox"
import React from "react"
import { useState } from "react"

import { CURRENT_CHAT, CURRENT_USER, TALKING_TO,chatController} from "../../Constants"


export async function ChatLoader({ params }) {
    const currentUser = JSON.parse(localStorage.getItem(CURRENT_USER))
    try
    {
        const promiseRequestInbox = fetch(`${chatController}/GetInbox/${currentUser.userId}`)
        const promiseRequestMessages = params.chatId != undefined ?
            fetch(`${chatController}/GetMessages/${params.chatId}`) : null
        
        const requestInbox = await promiseRequestInbox
        const requestMessages = promiseRequestMessages != null ? await promiseRequestMessages : null
        if (requestInbox.statusText == "OK")
        {
            const returnValue = {}
            const inboxArray = await requestInbox.json()
            returnValue['inboxArray'] = inboxArray
            returnValue['currentChat']=inboxArray.filter(c=>c.chatId===params.chatId)[0]
            if (requestMessages != null && requestMessages.statusText == 'OK')
            {
                const messages = await requestMessages.json()
                returnValue['currentChat']['messages']=messages
            }
            return returnValue
        }
    }
    catch (error)
    {
        console.log(error.message)
    }
    return null

}


export default function Home() {
    const navigate = useNavigate()
    
    const loaderData = useLoaderData()
    const [inboxArray, setInboxArray] = useState([])
    
    const [chatState, setChatState1] = useState({chatId:"",members:{},name:"", messages: [] })
    function setChatState(newValueOrFun)
    {
        if (typeof (newValueOrFun) !== 'function')
        {
            sessionStorage.setItem(CURRENT_CHAT,newValueOrFun)
        }
        setChatState1(newValueOrFun)
    }
    
    const currentUser = JSON.parse(localStorage.getItem(CURRENT_USER))
    const [query, setQuery] = useState("")
    async function searchForUser(query)
    {
       /*  try {
            const request = await fetch(userController + `/CheckUserExists?username=${query}`)
            if (request.ok)
            {
                const result = await request.text()
                if (result === 'true')
                {
                    navigate('/')
                    setChatState({
                            talkingTo: query
                        })
                }
            }
        }
        catch (error)
        {
            console.log(error.message)
        } */
 
    }
    
    React.useEffect(
        () => {
            if (loaderData != undefined && loaderData.inboxArray != undefined)
            {
                setInboxArray(loaderData.inboxArray.map(item => {
                    return <ChatCard username={item.name != '' ? item.name :
                        item.members.filter(it => it.username != currentUser.username)[0]}
                        chatUid={item.chatId} key={item.chatId} />
                }))
            if (loaderData.currentChat != undefined)
            {
                setChatState(
                    {
                        ...loaderData.currentChat,
                        messages: loaderData.currentChat.messages.map((msg)=> {
                            return <ChatBubble message={msg } key={msg.messageId} />
                        })
                    }
                )    
                }
            else {
                setChatState({messages:[]})
            }
                }
              
            }
        , [loaderData])
   
    React.useEffect(() => {
        if (currentUser == null) {
            navigate("/login")
        }
    }, [])
    
    return <div className="chat-container">

        <nav className="navbar">
        <SearchBar query={query} setQuery={setQuery} onQueryExecute={(ev) =>searchForUser(query)}/>
            <ul>
                <li>
                    <Button variant="text" sx={{
                        color: "white",
                        marginRight: "10px",
                        height: "70px"
                    }} onClick={
                        (event) => {
                            localStorage.removeItem(CURRENT_USER)
                            localStorage.removeItem(CURRENT_CHAT)
                            sessionStorage.removeItem(TALKING_TO)
                            navigate('/login')
                        }
                    }>
                        Log out
                    </Button>
                </li>
            </ul>
        </nav>
        <div className="chat-content">
            <div className="chat-history">
                {inboxArray}
            </div>
            <div className="chat-interface">
                    {sessionStorage.getItem(TALKING_TO)&& <div>{sessionStorage.getItem(TALKING_TO)}</div>}
                <div className="current-chat">
                    {chatState!=undefined&&chatState.messages!=undefined&&chatState.messages}
                </div>
                <MessageBox chatUid={chatState != undefined ? chatState.chatUid : undefined}
                        sendTo={sessionStorage.getItem(TALKING_TO)!= undefined ?sessionStorage.getItem(TALKING_TO) : undefined} onChooseEncryption={(
                        encryptionString
                        ) => {
                            switch (encryptionString)
                            {
                                case RAILFENCE:
                                    setCryptoAlg(railfence)
                                    break
                                case XXTEA_CBC:
                                    setCryptoAlg(xxtea_cbc)
                                    break
                                default:
                                    break;
                            }
                    }}/>
            </div>
        </div>
    </div>
}