import {useLoaderData,useNavigate } from "react-router-dom"
import SearchBar from "../../Components/Searchbar/Searchbar"
import './Chat.css'
import { Button } from "@mui/material"
import ChatCard from "../../Components/ChatCard/ChatCard"
import ChatBubble from "../../Components/ChatBubble/ChatBubble"
import MessageBox from "../../Components/MessageBox/MessageBox"
import React from "react"
import { useState, useRef } from "react"

import { CURRENT_CHAT, CURRENT_USER, TALKING_TO, chatController} from "../../Constants"


export async function ChatLoader({ params }) {
    const currentUser = JSON.parse(localStorage.getItem(CURRENT_USER))
    try
    {
        const promiseRequestInbox = fetch(`${chatController}/GetInbox/${currentUser.userId}`)
        const promiseRequestMessages = params.chatId != undefined ?
            fetch(`${chatController}/GetMessages/${params.chatId}`) : null
        
        const requestInbox = await promiseRequestInbox
        const requestMessages = promiseRequestMessages != null ? await promiseRequestMessages : null
        if (requestInbox.status === 200) {
            const returnValue = {}
            const inboxArray = await requestInbox.json()
            returnValue['inboxArray'] = inboxArray
            returnValue['currentChat'] = inboxArray.filter(c => c.chatId === params.chatId)[0]

            if (requestMessages != null && requestMessages.status === 200) {
                const messages = await requestMessages.json()
                returnValue['currentChat']['messages'] = messages
            }
            return returnValue
        }
        else {
            console.log("Couldn't get inboxes")
            return { inboxArray: [], currentChat: null }
        }
    }
    catch (error)
    {
        console.log(error.message)
        return { inboxArray: [], currentChat: null }
    }
    return null
}

export default function Home() {

    const navigate = useNavigate()
    const loaderData = useLoaderData()
    const [inboxArray, setInboxArray] = useState([])
    const [chatState, setChatState1] = useState({ chatId: "", members: {}, name: "", messages: [] })
    const currentUser = JSON.parse(localStorage.getItem(CURRENT_USER))
    const webSocketRef = useRef(null);

    const initWebSocket = (chatId) => {
        webSocketRef.current = new WebSocket(`wss://localhost:5173/chat?chatId=${chatId}`); // Replace with your WebSocket server endpoint
        console.log(`WebSocket connected od strane usera : ${localStorage.getItem(CURRENT_USER)}`);
        webSocketRef.current.onmessage = (event) => {
            const message = JSON.parse(event.data);
            setChatState((prevChatState) => ({
                ...prevChatState,
                messages: [...prevChatState.messages, message]
            }));
            console.log(`poslao se ${message}`);
            // Add logic to handle incoming message, update state, etc.
        };

        webSocketRef.current.onclose = () => {
            console.log('WebSocket disconnected');
            // Optionally reconnect or handle cleanup
        };

        // Don't forget to handle onerror and onopen if needed
    };

    const handleSendMessage = async (content, recipientId) => {
        // Send message to the server
        const message = {
            Content: content,
            SenderId: currentUser.userId
        };
        try {
            const response = await fetch(`${chatController}/SendMessage/${recipientId}`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify(message)
            });

            if (response.ok) {
                console.log("Message sent successfully:", response.data);
            } else {
                console.error("Failed to send message, server responded with status:", response.status);
            }
        } catch (error) {
            console.error("Failed to send message, error:", error);
        }
    };

    React.useEffect(() => {
        if (loaderData?.currentChat?.chatId) {
            initWebSocket(loaderData.currentChat.chatId);
        }

        return () => {
            if (webSocketRef.current) {
                webSocketRef.current.close(); // Close WebSocket when component unmounts
            }
        };
    }, [loaderData?.currentChat?.chatId]);


    const setCurrentChat = async (chat) => {
        // Check if messages are already loaded in the chat object
        if (chat.messages && chat.messages.length > 0) {
            setChatState1({
                ...chat,
                messages: chat.messages.map(msg => <ChatBubble message={msg} key={msg.messageId} />)
            });
        } 
    };


    function setChatState(newValueOrFun)
    {
        if (typeof (newValueOrFun) !== 'function')
        {
            sessionStorage.setItem(CURRENT_CHAT,newValueOrFun)
        }
        setChatState1(newValueOrFun)
    }

    React.useEffect(
        () => {
            if (loaderData != undefined && loaderData.inboxArray != undefined) {
                setInboxArray(loaderData.inboxArray.map(item => {
                    return <ChatCard
                        username={item.name != '' ? item.name :
                            item.members.filter(it => it.username != currentUser.username)[0]}
                        chatUid={item.chatId}
                        chatItem={item}
                        onChatSelect={setCurrentChat}
                    />
                }))
            }
            if (loaderData.currentChat != undefined)
            {
                setChatState(
                    {
                        ...loaderData.currentChat,
                        messages: loaderData.currentChat.messages.map((msg)=> {
                            return <ChatBubble message={msg} key={msg.messageId} />
                        })
                    }
                )    
                }
            else {
                setChatState({messages:[]})
            }

        }, [loaderData])
   
    React.useEffect(() => {
        if (currentUser == null) {
            navigate("/login")
        }
    }, [])
    
    return <div className="chat-container">
        <div className="chat-content">
            <div className="chat-history">
                {inboxArray}
            </div>
            <div className="chat-interface">
                   {sessionStorage.getItem(TALKING_TO)&& <div>{sessionStorage.getItem(TALKING_TO)}</div>}
                <div className="current-chat">
                    {chatState != undefined&&chatState.messages!=undefined && chatState.messages}
                </div>
                <MessageBox chatUid={chatState.chatId != undefined ? chatState.chatId : undefined}
                    sendTo={sessionStorage.getItem(TALKING_TO) != undefined ? sessionStorage.getItem(TALKING_TO) : undefined}
                    onSendMessage = {handleSendMessage}
                    onChooseEncryption={(
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